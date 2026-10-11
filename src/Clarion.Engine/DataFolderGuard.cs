using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Clarion.Engine;

/// <summary>
/// Locks the shared data folder so only administrators can write to it. The folder lives under ProgramData, where every account is
/// allowed to create files and folders. Without this, an ordinary account could create the change journal before Clarion ever ran with
/// administrator rights, and a later Revert would replay what that account wrote, as an administrator. It runs every time Clarion starts
/// with administrator rights, so a folder made by someone else is corrected too, and it only tightens: nothing is deleted or moved
/// except a folder that is really a link, which is renamed and replaced.
/// </summary>
[SupportedOSPlatform("windows")]
public static class DataFolderGuard
{
    private const int MaxEntries = 5000;

    /// <summary>Files Clarion reads back and acts on with administrator rights: the change journal, which Revert replays, and the restart queue.</summary>
    private static readonly string[] Replayed = ["journal.jsonl", "restart-queue.json"];

    public const string QuarantineFolder = "quarantine";

    private static readonly SecurityIdentifier LocalSystem = new(WellKnownSidType.LocalSystemSid, null);
    private static readonly SecurityIdentifier Administrators = new(WellKnownSidType.BuiltinAdministratorsSid, null);
    private static readonly SecurityIdentifier Users = new(WellKnownSidType.BuiltinUsersSid, null);

    /// <summary>Accounts whose ownership of a file in the folder is believed. The person running with administrator rights counts.</summary>
    public static bool IsTrustedOwner(SecurityIdentifier? owner)
    {
        if (owner is null) return false;
        if (owner.Equals(LocalSystem) || owner.Equals(Administrators)) return true;
        using var identity = WindowsIdentity.GetCurrent();
        return identity.User is { } me && owner.Equals(me);
    }

    /// <summary>Returns what it had to correct, in plain words, or nothing when the folder was already right.</summary>
    public static IReadOnlyList<string> Secure(string folder, TimeProvider? clock = null)
    {
        var notes = new List<string>();
        try
        {
            var root = new DirectoryInfo(folder);

            // A folder that is a link would send every read and write somewhere else, and changing its permissions would change the
            // permissions of wherever it points. It is renamed, which moves only the link, and a real folder takes its place.
            if (root.Exists && root.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                var stamp = (clock ?? TimeProvider.System).GetUtcNow().ToString("yyyyMMdd-HHmmss");
                var aside = folder.TrimEnd('\\', '/') + ".link-" + stamp;
                Directory.Move(folder, aside);
                notes.Add($"The data folder was a link, so it was renamed to {aside} and a real folder was made.");
                root = new DirectoryInfo(folder);
            }

            if (!root.Exists) root.Create();

            if (TakeOwnership(root)) notes.Add("The data folder belonged to another account. Ownership now belongs to administrators.");
            if (!IsLocked(root))
            {
                root.SetAccessControl(Locked());
                notes.Add("The data folder allowed ordinary accounts to add files. Only administrators can write to it now.");
            }

            // The files that are acted on come first and by name, so a crowd of other files cannot push them past the cap below.
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            foreach (var name in Replayed) SetAsideIfUntrusted(root, name, now, notes);

            var seen = 0;
            var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
            foreach (var entry in root.EnumerateFileSystemInfos("*", options))
            {
                if (++seen > MaxEntries)
                {
                    notes.Add($"The data folder holds more than {MaxEntries} entries, so the permissions of the rest were not checked.");
                    break;
                }
                try
                {
                    if (ResetEntry(entry)) notes.Add($"Reset the permissions of {entry.Name}, which did not follow the folder's.");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
                {
                    notes.Add($"Could not reset the permissions of {entry.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or PrivilegeNotHeldException)
        {
            notes.Add($"Could not lock the data folder: {ex.Message}");
        }
        return notes;
    }

    /// <summary>
    /// A replayed file that another account made, or gave itself rights on, or that is a link, is moved aside and not used. Resetting its
    /// permissions would leave its content, and that content would then run as administrator. It is moved and not deleted, so a file that
    /// was fine all along is still there to be put back by hand.
    /// </summary>
    private static void SetAsideIfUntrusted(DirectoryInfo root, string name, DateTimeOffset now, List<string> notes)
    {
        var path = Path.Combine(root.FullName, name);
        try
        {
            var file = new FileInfo(path);
            // A file that is not there reports attributes of -1, which would read as every flag including link, so it is ruled out first.
            if ((int)file.Attributes == -1) return;
            var link = file.Attributes.HasFlag(FileAttributes.ReparsePoint);
            var security = file.GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner);
            var owner = security.GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier;
            var ownRules = security.GetAccessRules(true, false, typeof(SecurityIdentifier)).Count;
            // Only the system and administrators count here. The signed-in person's own account is no proof, because a program running
            // at normal rights under that account could have made the file. Clarion with administrator rights owns what it writes as Administrators.
            var administrative = owner is not null && (owner.Equals(LocalSystem) || owner.Equals(Administrators));
            if (!link && administrative && ownRules == 0 && !security.AreAccessRulesProtected) return;

            var stamp = now.ToString("yyyyMMdd-HHmmss");
            var aside = Path.Combine(root.FullName, QuarantineFolder);
            string target;
            try
            {
                Directory.CreateDirectory(aside);
                target = Path.Combine(aside, $"{name}.{stamp}-{Guid.NewGuid().ToString("N")[..8]}");
                File.Move(path, target);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // If the folder for it cannot be used, it is at least renamed where it is, so it is no longer read under its own name.
                target = path + ".untrusted-" + stamp;
                File.Move(path, target);
            }
            if (!link) ResetEntry(new FileInfo(target));
            notes.Add($"{name} was made by another account or had permissions of its own, so it was moved to {target} and is not used. Nothing was deleted.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            notes.Add($"Could not check {name}: {ex.Message}");
        }
    }

    private static DirectorySecurity Locked()
    {
        var security = new DirectorySecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        const InheritanceFlags both = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
        security.AddAccessRule(new FileSystemAccessRule(LocalSystem, FileSystemRights.FullControl, both, PropagationFlags.None, AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(Administrators, FileSystemRights.FullControl, both, PropagationFlags.None, AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(Users, FileSystemRights.ReadAndExecute, both, PropagationFlags.None, AccessControlType.Allow));
        return security;
    }

    /// <summary>True when the folder already has the protected, administrators-write permissions and nothing more.</summary>
    private static bool IsLocked(DirectoryInfo folder)
    {
        var security = folder.GetAccessControl(AccessControlSections.Access);
        if (!security.AreAccessRulesProtected) return false;
        foreach (FileSystemAccessRule rule in security.GetAccessRules(true, false, typeof(SecurityIdentifier)))
        {
            var sid = (SecurityIdentifier)rule.IdentityReference;
            if (sid.Equals(LocalSystem) || sid.Equals(Administrators))
            {
                if (rule.AccessControlType == AccessControlType.Deny) return false;
                continue;
            }
            if (sid.Equals(Users) && rule.AccessControlType == AccessControlType.Allow && !HasWrite(rule.FileSystemRights)) continue;
            // Anyone else, or a deny, means the folder is not as it should be. The current account is the owner's own business below.
            return false;
        }
        return true;
    }

    private static bool HasWrite(FileSystemRights rights) =>
        (rights & (FileSystemRights.WriteData | FileSystemRights.AppendData | FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles
                   | FileSystemRights.ChangePermissions | FileSystemRights.TakeOwnership)) != 0;

    /// <summary>Makes administrators the owner when it is anyone else. An owner can always change the permissions, so it matters.</summary>
    private static bool TakeOwnership(FileSystemInfo entry)
    {
        var current = Owner(entry);
        if (IsTrustedOwner(current)) return false;
        try
        {
            switch (entry)
            {
                case DirectoryInfo d:
                    var ds = new DirectorySecurity();
                    ds.SetOwner(Administrators);
                    d.SetAccessControl(ds);
                    break;
                case FileInfo f:
                    var fs = new FileSecurity();
                    fs.SetOwner(Administrators);
                    f.SetAccessControl(fs);
                    break;
            }
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or PrivilegeNotHeldException)
        {
            // Without administrator rights the owner cannot be changed. The permissions below are still tightened.
            return false;
        }
    }

    private static SecurityIdentifier? Owner(FileSystemInfo entry) => entry switch
    {
        DirectoryInfo d => d.GetAccessControl(AccessControlSections.Owner).GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier,
        FileInfo f => f.GetAccessControl(AccessControlSections.Owner).GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier,
        _ => null,
    };

    /// <summary>Sets a file or folder inside the data folder back to the permissions it inherits. False when it already did.</summary>
    private static bool ResetEntry(FileSystemInfo entry)
    {
        FileSystemSecurity security = entry is DirectoryInfo d
            ? d.GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner)
            : ((FileInfo)entry).GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner);
        var owner = security.GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier;
        var explicitRules = security.GetAccessRules(true, false, typeof(SecurityIdentifier)).Count;
        if (explicitRules == 0 && !security.AreAccessRulesProtected && IsTrustedOwner(owner)) return false;

        // The owner first, since an owner who is not trusted could undo the rest.
        TakeOwnership(entry);
        // An empty set of rules is only written when it is marked as changed, which turning inheritance on does. The result is no rules of
        // its own and whatever the folder passes down.
        switch (entry)
        {
            case DirectoryInfo dir:
                var inheritedDir = new DirectorySecurity();
                inheritedDir.SetAccessRuleProtection(isProtected: false, preserveInheritance: false);
                dir.SetAccessControl(inheritedDir);
                break;
            case FileInfo file:
                var inheritedFile = new FileSecurity();
                inheritedFile.SetAccessRuleProtection(isProtected: false, preserveInheritance: false);
                file.SetAccessControl(inheritedFile);
                break;
        }
        return true;
    }
}

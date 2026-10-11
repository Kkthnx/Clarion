using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using Clarion.Engine;

namespace Clarion.Tests;

[SupportedOSPlatform("windows")]
public sealed class DataFolderGuardTests : IDisposable
{
    private static readonly SecurityIdentifier Users = new(WellKnownSidType.BuiltinUsersSid, null);
    private static readonly SecurityIdentifier Administrators = new(WellKnownSidType.BuiltinAdministratorsSid, null);
    private static readonly SecurityIdentifier LocalSystem = new(WellKnownSidType.LocalSystemSid, null);

    private readonly string _root = Path.Combine(Path.GetTempPath(), "clarion-guard-" + Guid.NewGuid().ToString("N"));

    public DataFolderGuardTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        // The guard removes the rights the owner would need to delete the folder, so they are given back before cleaning up.
        try
        {
            var me = WindowsIdentity.GetCurrent().User!;
            foreach (var path in Directory.EnumerateDirectories(_root, "*", SearchOption.AllDirectories).Prepend(_root))
            {
                try
                {
                    var security = new DirectorySecurity();
                    security.AddAccessRule(new FileSystemAccessRule(me, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    new DirectoryInfo(path).SetAccessControl(security);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
            Directory.Delete(_root, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    private string NewFolder(string name = "data")
    {
        var path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static IEnumerable<FileSystemAccessRule> Rules(string path) =>
        new DirectoryInfo(path).GetAccessControl(AccessControlSections.Access)
            .GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>();

    private static bool CanWrite(FileSystemRights rights) =>
        (rights & (FileSystemRights.WriteData | FileSystemRights.AppendData | FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories | FileSystemRights.Delete)) != 0;

    [Fact]
    public void Ordinary_accounts_can_read_the_folder_but_not_write_to_it()
    {
        var folder = NewFolder();

        DataFolderGuard.Secure(folder);

        var rules = Rules(folder).Where(r => r.AccessControlType == AccessControlType.Allow).ToList();
        Assert.Contains(rules, r => r.IdentityReference.Equals(Users) && (r.FileSystemRights & FileSystemRights.ReadData) != 0);
        Assert.DoesNotContain(rules, r => r.IdentityReference.Equals(Users) && CanWrite(r.FileSystemRights));
        Assert.Contains(rules, r => r.IdentityReference.Equals(Administrators) && r.FileSystemRights.HasFlag(FileSystemRights.FullControl));
        Assert.Contains(rules, r => r.IdentityReference.Equals(LocalSystem) && r.FileSystemRights.HasFlag(FileSystemRights.FullControl));
        Assert.True(new DirectoryInfo(folder).GetAccessControl(AccessControlSections.Access).AreAccessRulesProtected);
    }

    [Fact]
    public void A_folder_that_is_already_right_is_left_alone()
    {
        var folder = NewFolder();
        DataFolderGuard.Secure(folder);

        var second = DataFolderGuard.Secure(folder);

        Assert.Empty(second);
    }

    [Fact]
    public void A_missing_folder_is_made_locked()
    {
        var folder = Path.Combine(_root, "not-there-yet");

        DataFolderGuard.Secure(folder);

        Assert.True(Directory.Exists(folder));
        Assert.DoesNotContain(Rules(folder), r => r.IdentityReference.Equals(Users) && r.AccessControlType == AccessControlType.Allow && CanWrite(r.FileSystemRights));
    }

    private static string[] Quarantined(string folder) =>
        Directory.Exists(Path.Combine(folder, DataFolderGuard.QuarantineFolder))
            ? Directory.GetFiles(Path.Combine(folder, DataFolderGuard.QuarantineFolder)) : [];

    [Fact]
    public void A_journal_someone_gave_themselves_rights_on_is_set_aside_and_not_used()
    {
        var folder = NewFolder();
        var journal = Path.Combine(folder, "journal.jsonl");
        File.WriteAllText(journal, "{}\n");
        var planted = new FileSecurity();
        planted.AddAccessRule(new FileSystemAccessRule(Users, FileSystemRights.FullControl, AccessControlType.Allow));
        new FileInfo(journal).SetAccessControl(planted);

        var notes = DataFolderGuard.Secure(folder);

        // Its bytes would still be replayed as administrator if it stayed under its own name, so it is moved, not just reset.
        Assert.False(File.Exists(journal));
        var kept = Assert.Single(Quarantined(folder));
        Assert.StartsWith("journal.jsonl.", Path.GetFileName(kept), StringComparison.Ordinal);
        Assert.Equal("{}\n", File.ReadAllText(kept));
        var after = new FileInfo(kept).GetAccessControl(AccessControlSections.Access);
        Assert.Empty(after.GetAccessRules(true, false, typeof(SecurityIdentifier)));
        Assert.Contains(notes, n => n.Contains("journal.jsonl", StringComparison.Ordinal) && n.Contains("Nothing was deleted", StringComparison.Ordinal));
    }

    [Fact]
    public void A_journal_owned_by_an_ordinary_account_is_set_aside()
    {
        // A journal made by the signed-in account itself cannot be told from one planted by a program running as that account.
        if (!WindowsMachine.IsElevated()) return;
        var folder = NewFolder();
        var journal = Path.Combine(folder, "journal.jsonl");
        File.WriteAllText(journal, "{}\n");
        var theirs = new FileSecurity();
        theirs.SetOwner(Users);
        try { new FileInfo(journal).SetAccessControl(theirs); }
        catch (InvalidOperationException) { return; } // some machines do not allow handing a file to another owner

        DataFolderGuard.Secure(folder);

        Assert.False(File.Exists(journal));
        var kept = Assert.Single(Quarantined(folder));
        var owner = new FileInfo(kept).GetAccessControl(AccessControlSections.Owner).GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier;
        Assert.True(DataFolderGuard.IsTrustedOwner(owner), $"owner is {owner}");
    }

    [Fact]
    public void A_journal_clarion_wrote_with_administrator_rights_stays_where_it_is()
    {
        // Needs administrator rights to write into the locked folder, as Clarion itself does.
        if (!WindowsMachine.IsElevated()) return;
        var folder = NewFolder();
        DataFolderGuard.Secure(folder);
        var journal = Path.Combine(folder, "journal.jsonl");
        File.WriteAllText(journal, "{}\n");
        var owner = new FileInfo(journal).GetAccessControl(AccessControlSections.Owner).GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier;
        // On a PC set to make the creator the owner instead of Administrators, the file is not told from a planted one and this does not apply.
        if (owner is null || !owner.Equals(Administrators)) return;

        var notes = DataFolderGuard.Secure(folder);

        Assert.True(File.Exists(journal));
        Assert.Empty(Quarantined(folder));
        Assert.Empty(notes);
    }

    [Fact]
    public void A_crowd_of_other_files_cannot_hide_a_planted_journal()
    {
        var folder = NewFolder();
        for (var i = 0; i < 5100; i++) File.WriteAllBytes(Path.Combine(folder, $"decoy-{i:D5}.tmp"), []);
        var journal = Path.Combine(folder, "journal.jsonl");
        File.WriteAllText(journal, "{}\n");
        var planted = new FileSecurity();
        planted.AddAccessRule(new FileSystemAccessRule(Users, FileSystemRights.FullControl, AccessControlType.Allow));
        new FileInfo(journal).SetAccessControl(planted);

        var notes = DataFolderGuard.Secure(folder);

        Assert.False(File.Exists(journal));
        Assert.Single(Quarantined(folder));
        Assert.Contains(notes, n => n.Contains("more than 5000", StringComparison.Ordinal));
    }
    [Fact]
    public void A_folder_that_is_a_link_is_renamed_and_replaced_without_touching_where_it_pointed()
    {
        var elsewhere = NewFolder("elsewhere");
        File.WriteAllText(Path.Combine(elsewhere, "theirs.txt"), "x");
        var before = Rules(elsewhere).Select(r => (r.IdentityReference.Value, r.FileSystemRights)).ToList();
        var link = Path.Combine(_root, "linked");
        var psi = new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{elsewhere}\"") { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var p = Process.Start(psi)!) { p.StandardOutput.ReadToEnd(); p.StandardError.ReadToEnd(); p.WaitForExit(); }
        Assert.True(new DirectoryInfo(link).Attributes.HasFlag(FileAttributes.ReparsePoint));

        var notes = DataFolderGuard.Secure(link);

        Assert.False(new DirectoryInfo(link).Attributes.HasFlag(FileAttributes.ReparsePoint));
        Assert.Contains(notes, n => n.Contains("was a link", StringComparison.Ordinal));
        Assert.True(File.Exists(Path.Combine(elsewhere, "theirs.txt")));
        var after = Rules(elsewhere).Select(r => (r.IdentityReference.Value, r.FileSystemRights)).ToList();
        Assert.Equal(before, after);
    }

    [Fact]
    public void Only_administrators_system_and_the_running_account_are_believed_as_owners()
    {
        Assert.True(DataFolderGuard.IsTrustedOwner(Administrators));
        Assert.True(DataFolderGuard.IsTrustedOwner(LocalSystem));
        Assert.True(DataFolderGuard.IsTrustedOwner(WindowsIdentity.GetCurrent().User));
        Assert.False(DataFolderGuard.IsTrustedOwner(Users));
        Assert.False(DataFolderGuard.IsTrustedOwner(new SecurityIdentifier(WellKnownSidType.WorldSid, null)));
        Assert.False(DataFolderGuard.IsTrustedOwner(null));
    }
}

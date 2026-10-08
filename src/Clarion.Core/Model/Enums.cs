namespace Clarion.Core.Model;

public enum Evidence { Proven, Situational, Cosmetic, Unproven }

public enum RiskLevel { Safe, Low, Medium, High }

public enum TweakScope { User, Machine }

public enum RegistryHive { CurrentUser, LocalMachine }

public enum RegistryKind { String, ExpandString, DWord, QWord, Binary }

public enum TweakState { NotApplied, Applied, Partial, Unavailable }

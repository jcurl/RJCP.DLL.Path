// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0056:Use index operator", Justification = ".NET 4.x compatibility")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = ".NET 4.x compatibility")]
[assembly: SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = ".NET Core only feature")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unix P/Invoke API", Scope = "namespaceanddescendants", Target = "~N:RJCP.IO.Files.Exe.Unix")]
[assembly: SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "P/Invoke API", Scope = "namespaceanddescendants", Target = "~N:RJCP.IO.Native.Win32")]
[assembly: SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "P/Invoke API", Scope = "namespaceanddescendants", Target = "~N:RJCP.IO.Files.Exe.Win32")]

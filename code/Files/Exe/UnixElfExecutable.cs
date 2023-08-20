namespace RJCP.IO.Files.Exe
{
    using System;

    internal sealed class UnixElfExecutable : FileExecutable
    {
        internal static UnixElfExecutable GetUnixFile(string path)
        {
            throw new NotImplementedException();
        }

        internal UnixElfExecutable() { }

        public override FileMachineType MachineType { get; }

        public override FileTargetOs TargetOs { get; }

        public override bool IsLittleEndian { get; }

        public override bool IsExe { get; }

        public override bool IsDll { get; }

        public override int ArchitectureSize { get; }
    }
}

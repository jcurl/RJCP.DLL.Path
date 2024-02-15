namespace RJCP.IO.Files.Exe
{
    using System.IO;
    using RJCP.CodeQuality;

    public static class UnixElfExecutableAccessor
    {
        private const string Assemblyname = "RJCP.IO.Path";
        private const string TypeName = "RJCP.IO.Files.Exe.UnixElfExecutable";
        private static readonly PrivateType AccType = new(Assemblyname, TypeName);

        public static UnixElfExecutable GetFile(BinaryReader br)
        {
            return (UnixElfExecutable)AccessorBase.InvokeStatic(AccType, nameof(GetFile), br);
        }
    }
}

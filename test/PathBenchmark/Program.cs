namespace RJCP.IO
{
    using BenchmarkDotNet.Running;

    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}

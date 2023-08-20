namespace RJCP.IO.Files.Exe
{
    /// <summary>
    /// The Machine Type for the Executable File.
    /// </summary>
    public enum FileMachineType
    {
        /// <summary>
        /// The machine type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The machine type is 32-bit ARM (e.g. Cortex A9).
        /// </summary>
        Arm,

        /// <summary>
        /// The machine type is 64-bit ARM.
        /// </summary>
        Arm64,

        /// <summary>
        /// The machine type is ARM Thumb.
        /// </summary>
        ArmThumb,

        /// <summary>
        /// The machine type is ARM Thumb-2
        /// </summary>
        ArmThumb2,

        /// <summary>
        /// The machine type is Intel x86 32-bit.
        /// </summary>
        Intel386,

        /// <summary>
        /// The machine type is Itanium 64-bit.
        /// </summary>
        Itanium64,

        /// <summary>
        /// The machine type is AMD x86 64-bit.
        /// </summary>
        Amd64,

        /// <summary>
        /// The machine type is DEC Alpha.
        /// </summary>
        Alpha
    }
}

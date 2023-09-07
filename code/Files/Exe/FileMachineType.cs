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
        Alpha,

        /// <summary>
        /// The machine type is Power PC.
        /// </summary>
        PowerPC,

        /// <summary>
        /// The machine type is PowerPC 64-bit.
        /// </summary>
        PowerPC64,

        /// <summary>
        /// The machine type is Sparc.
        /// </summary>
        Sparc,

        /// <summary>
        /// The machine type is SparcV9.
        /// </summary>
        SparcV9,

        /// <summary>
        /// The machine type is Risc-V.
        /// </summary>
        RiscV,

        /// <summary>
        /// The machine type is MIPS.
        /// </summary>
        Mips,

        /// <summary>
        /// The machine type is HP-PARISC.
        /// </summary>
        PARISC,

        /// <summary>
        /// The machine type is IBM S390x.
        /// </summary>
        S390x,

        /// <summary>
        /// The machine type is Motorola 68000.
        /// </summary>
        M68k,

        /// <summary>
        /// The machine type is Digital VAX.
        /// </summary>
        Vax,

        /// <summary>
        /// The machine type is Super H.
        /// </summary>
        SuperH,
    }
}

# Introduction <!-- omit in toc -->

The `FileExecutable` class is used to read an expected executable and to
interpret the contents for:

* Windows Executable with the PE format; or
* Unix Exectuable with the ELF format.

- [1. Unix ELF Executable Format](#1-unix-elf-executable-format)
  - [1.1. Executable Format](#11-executable-format)
  - [1.2. Dynamic Linked Library](#12-dynamic-linked-library)
  - [1.3. Observations for ELF parsing](#13-observations-for-elf-parsing)
    - [1.3.1. Linux](#131-linux)
      - [1.3.1.1. Libraries and Executable Dual Purpose](#1311-libraries-and-executable-dual-purpose)
    - [1.3.2. OpenBSD](#132-openbsd)
      - [1.3.2.1. Position Independent Executable Flag](#1321-position-independent-executable-flag)
      - [1.3.2.2. OpenBSD Libraries](#1322-openbsd-libraries)

## 1. Unix ELF Executable Format

Some anecdotal information is needed to interpret if the ELF file is an
executable or is a dynamic link library.

References:

* [Tool Interface Standard (TIS) Executable and Linking Format (ELF)
  Specification, Version 1.2](https://refspecs.linuxfoundation.org/elf/elf.pdf)
* [Executable and Linkable Format,
  Wikipedia](https://en.wikipedia.org/wiki/Executable_and_Linkable_Format)

It's generally not possible to try and determine for which operating system the
file is compiled for.

### 1.1. Executable Format

The ELF Header contains the basic information about the file, and this class is
interested in one of two different executable types, referenced by `e_type`.

* `ET_EXEC` - an executable, that the operating system must relocate and fix
  offsets in the code depending on the location in memory it is loaded. If this
  flag is set by
* `ET_DYN` - a shared object, or also may be an executable with position
  independent code.

This section concentrates on the logic for determining if an ELF file with
`ET_DYN` is an executable.

If the Program Header contains the section `PT_INTERP`, this object can be
considered to be an executable. This is most commonly observed on Linux.

If the section `PT_DYNAMIC` exists, and it contains `DT_FLAGS_1` which has the
bit `DF_1_PIE` set, then this is also an executable.

If the section `PT_INTERP` doesn't exist, we check if it exports symbols (I
don't think an ELF executable should export symbols):

* If `PT_DYNAMIC` exists, and it contains `DT_SONAME`, this is not an executable
  (it's a shared object). Note, the inverse is not true, shared objects may also
  have `PT_INTERP`.
* If `SHT_SYMTAB` exists in the section headers and it's called `.symtab`, this is
  not an executable either (it's a shared object and exports symbols for linking)

### 1.2. Dynamic Linked Library

The ELF header must contain the `e_type` of `ET_DYN`. This information alone is
not enough to indicate that this is a shared library.

If the section `PT_DYNAMIC` exists, and it contains `DT_FLAGS_1` which has the
bit `DF_1_PIE` set, then this is not a shared library.

If the `PT_DYNAMIC` exists, and it contains `DT_SONAME`, then this is a shared
library.

### 1.3. Observations for ELF parsing

#### 1.3.1. Linux

Linux always appears to have `DT_SONAME` for libraries.

Executables are either `ET_EXE` or `ET_DYN`. All exe's appear to have
`PT_INTERP`.

##### 1.3.1.1. Libraries and Executable Dual Purpose

It is possible on Linux to have a binary that is an executable, as well as a
shared library. The most common case is `libc`, which if executed, provides
version information about itself.

If the library has `DT_SONAME`, then it is a shared object (DLL).

#### 1.3.2. OpenBSD

##### 1.3.2.1. Position Independent Executable Flag

For OpenBSD, binaries are executable and may also not have the `PT_INTERP`. It
is observed that they don't have `DT_SONAME` and mostly contain `DT_FLAGS_1`
with `DF_1_PIE`.

```text
Dynamic section at offset 0x1e328 contains 20 entries:
  Tag        Type                         Name/Value
 0x000000006ffffffb (FLAGS_1)            Flags: PIE
```

If we look at [file/src/readelf.h
L500](https://github.com/file/file/blob/445f387/src/readelf.h#L500), this
defines `DT_FLAGS_1`, and the flag `DF_1_PIE` is defined at [file/src/readelf.h
L543](https://github.com/file/file/blob/445f387/src/readelf.h#L543).

This classifies further binaries, but it does not appear to be enough. Some
binaries have no `PT_INTERP`, no `DT_SONAME`, no `DT_FLAGS_1`.

In case a binary does not have `PT_INTERP`, or `DT_FLAGS_1` with `DF_1_PIE`, it
might still be a binary. Most system binaries (e.g. the tool `/bin/cat`) appear
like this. We will consider it an executable if it doesn't have `SHT_SYMTAB`
with the name `.symtab`.

Note, that when compiling my own binary (a simple 'hello world'), it does have:

* `ET_DYN` dynamic linking, for position independent code;
* `PT_INTERP` pointing to `/usr/libexec/ldso`;
* `DT_FLAGS_1` with `DF_1_PIE` set;
* `SHT_SYMTAB` with the name `.symtab`. Thus, this check is only a last resort,
  as observed, tools such as `/bin/cat` don't have this section (or the others).

##### 1.3.2.2. OpenBSD Libraries

It has been observed that some libraries (e.g. `libpciaccess.so.2.1`) do not
have a `PT_INTERP`, do not have a `DT_SONAME`. Thus interpreted as an executable
(which it isn't). And so it is believed to be an executable and not a DLL.

The section headers are scanned for `SHT_SYMTAB` (a symbol table that is used
for dynamic linking) that has the name `.symtab`. Executables appear not to have
this.

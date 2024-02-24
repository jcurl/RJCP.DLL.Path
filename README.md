# RJCP.DLL.Path <!-- omit in toc -->

This library provides Linux and Windows path manipulation based on strings only.
It doesn't rely on the Operating System, so it only attempts to handle high
level path features.

- [1. Features](#1-features)
  - [1.1. Path Strings](#11-path-strings)
    - [1.1.1. Windows](#111-windows)
    - [1.1.2. Linux](#112-linux)
  - [1.2. Detailed File System Information](#12-detailed-file-system-information)
  - [1.3. FileExecutable - Information about an Executable File](#13-fileexecutable---information-about-an-executable-file)
- [2. Release History](#2-release-history)
  - [2.1. Version 0.2.1](#21-version-021)
  - [2.2. Version 0.2.0](#22-version-020)

## 1. Features

### 1.1. Path Strings

The library provides:

- Operating system agnostic handling. Means that you can write unit tests for
  Linux path handling, or Windows path handling, and can run your unit tests on
  all OSes.
- Normalization of paths in their canonical form. Remove the `.` or `..` from
  the path automatically. Relative paths remain, and if the path starts off as
  being pinned (it references the path `\`) and tries to go beyond this, an
  exception is raised.
- On Windows, it understands UNC and DOS style paths.
  - DOS paths on Windows maintain the path per drive letter. So it is still
    possible to define a relative path, like `C:foo`.
  - UNC paths are `\\server\share`, and one can't go up a directory.

.NET Core provides more methods to handle paths. If this satisfies your needs,
then there is no need to move over to this library.

- A path is it's own type `Path`. This makes it easier to separate from strings.
- Creating a `Path` changes depending on the OS, unless explicitly created with
  `WindowsPath` or `UnixPath`.
- Contains functionality that older versions of .NET doesn't have.
- Performs additional checks on relative paths when normalizing, to ensure that
  it remains consistent (i.e. it won't allow going up sufficient parent
  directories that exceeds the root directory).

#### 1.1.1. Windows

It supports DOS paths (`C:\`) and UNC paths (`\\server\share` or `\\.\COM10`).

#### 1.1.2. Linux

All paths are treated the same, with the root path being `/`.

### 1.2. Detailed File System Information

Obtain detailed information about a file (the inode information, volume) via
`FileSystemNodeInfo`. This can allow for comparing of two paths point to the
same file, as reported by the file system, regardless if they're symbolic links,
or hard links.

### 1.3. FileExecutable - Information about an Executable File

Supports reading a Windows PE file, or a Linux ELF file, to get information
about the executable binary. Get file information with
`FileExecutable.GetFile()`.

Newer versions of .NET have functionality to read PE files, but only on Windows.
No support for ELF files exist at this time.

## 2. Release History

### 2.1. Version 0.2.1

Features:

- FileSystemNodeInfo: Allow comparison of two paths if they resolve to the same
  path (DOTNET-852, DOTNET-866, DOTNET-868, DOTNET-897)
- FileSystemNodeInfo: Recurse on Windows XP (DOTNET-852)
- FileExecutable (WinExecutable): A mechanism to test if a file is an executable
  (DOTNET-856, DOTNET-905)
- UnixElfExecutable: Read an ELF file (DOTNET-857)

Quality:

- Add README.md reference to NuGet package (DOTNET-818)
- Kernel32: Make ACCESS_MASK readonly (DOTNET-905)
- Path: Return concrete types instead of parent types (DOTNET-905)
- Upgrade from .NET Standard 2.1 to .NET 6.0 (DOTNET-936,  DOTNET-937,
  DOTNET-938, DOTNET-942, DOTNET-945, DOTNET-958, DOTNET-959)

### 2.2. Version 0.2.0

- Initial Release

# RJCP.DLL.Path

This library provides Linux and Windows path manipulation based on strings only.
It doesn't rely on the Operating System, so it only attempts to handle high
level path features.

The library provides:

* Operating system agnostic handling. Means that you can write unit tests for
  Linux path handling, or Windows path handling, and can run your unit tests on
  all OSes.
* Normalization of paths in their canonical form. Remove the `.` or `..` from
  the path automatically. Relative paths remain, and if the path starts off as
  being pinned (it references the path `\`) and tries to go beyond this, an
  exception is raised.
* On Windows, it understands UNC and DOS style paths.
  * DOS paths on Windows maintain the path per drive letter. So it is still
    possible to define a relative path, like `C:foo`.
  * UNC paths are `\\server\share`, and one can't go up a directory.

.NET Core provides more methods to handle paths. If this satisfies your needs,
then there is no need to move over to this library.

* A path is it's own type `Path`. This makes it easier to separate from strings.
* Creating a `Path` changes depending on the OS, unless explicitly created with
  `WindowsPath` or `UnixPath`.
* Contains functionality that older versions of .NET doesn't have.
* Performs additional checks on relative paths when normalizing, to ensure that
  it remains consistent (i.e. it won't allow going up sufficient parent
  directories that exceeds the root directory).

## Windows

It supports DOS paths (`C:\`) and UNC paths (`\\server\share` or `\\.\COM10`).

## Linux

All paths are treated the same, with the root path being `/`.

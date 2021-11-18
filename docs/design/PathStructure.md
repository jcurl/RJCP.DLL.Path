# Path Structure

This document describes the internal representation of a path. A path is a stack
of nodes, each node is either a directory element, or a file element. The `Path`
classes don't directly distinguish what is a directory or what is a file - that
is up to the Operating system.

The path for a `WindowsPath` and a `UnixPath` are similar, but different enough
to warrant two different (but again, very similar) implementations. The
separation allows for optimizations to be applied easily while maintaining
readership, at the (opinionatedly small) cost of maintenance. This cost is
offset by having unit test cases that, despite two different implementations,
should ensure code works as expected.

## The Path Stack

Both implementations have the concept of the `RootVolume`, `IsPinned` and the
`PathStack`. For Windows, the `RootVolume` is the drive letter, or the UNC path,
or empty. For unix paths, the `RootVolume` is always empty.

A path is referenced from the root of the `RootVolume`. If the `RootVolume` is
empty, then it depends on:

* Windows - The drive letter is not provided, so it's not clear to which drive
  the absolute path applies. Windows maintains internally one path per drive
  letter. So a drive letter behaves as a kind of namespace shortcut.
* Unix - The `RootVolume` is always empty, there is only one namespace, and
  `IsPinned` is used to determine if the path is absolute from `/`, or if it is
  relative from some other path directory.

In general, the path stack is an array of path nodes.

* If it is zero length, then the path is empty.
* If it has `string.Empty` at the end, then that is equivalent to saying the
  path is a directory, and a path directory separator is appended at the end.
  The `string.Empty` is equivalent to the current directory, e.g. `.`.
* All other elements build up the path.

## Windows Path

A Windows path string is deconstructed into the following elements:

| Path            | RootVolume   | IsPinned | IsUnc | IsDos | PathStack |
|-----------------|--------------|----------|-------|-------|-----------|
| `""`            | `""`         | false    | false | false | [0]       |
| `\`             | `""`         | true     | false | false | ""        |
| `C:`            | `C:`         | false    | false | true  | [0]       |
| `C:\`           | `C:`         | true     | false | true  | ""        |
| `foo`           | `""`         | false    | false | false | foo       |
| `foo\`          | `""`         | false    | false | false | foo, ""   |
| `C:foo`         | `C:`         | false    | false | true  | foo       |
| `C:\foo`        | `C:`         | true     | false | true  | foo       |
| `C:\foo\`       | `C:`         | true     | false | true  | foo, ""   |
| `\\srv`         | `\\srv`      | true     | true  | false | [0]       |
| `\\srv\s`       | `\\srv\s`    | true     | true  | false | [0]       |
| `\\srv\s\`      | `\\srv\s`    | true     | true  | false | ""        |
| `\\srv\s\foo`   | `\\srv\s`    | true     | true  | false | foo       |
| `\\srv\s\foo\`  | `\\srv\s`    | true     | true  | false | foo, ""   |
| `\\srv\s\foo\b` | `\\srv\s`    | true     | true  | false | foo, b    |

* `RootVolume` is the drive letter or the UNC path if provided. It may be blank
  of course.
* `IsPinned` indicates if the path is referenced from the root of the
  `RootVolume` or the current OS root volume if not specified in the path.
* `IsDos` indicates this is a path with a DOS drive letter. It may, or may not
  be pinned. It's quite valid to have a path like `C:foo`, where this is a
  relative path to the current path the OS maintains for the `C:` drive, which
  can be a different path than `D:foo`. Try it on the command line, `cd D:foo`
  when on the `C:` drive will enter the `foo` directory on the `D:` drive, but
  the current `C:` drive path won't change.
* `IsUnc` indicates this satisfies Universal Naming Convention rules. Note, not
  full rules are applied when parsing, we leave it up to the operating system to
  decide what is valid or not, as the OS may take UTF8 and convert that to a
  valid string at lower levels (e.g. via DNS libraries).

The `PathStack` is everything to the right of the root volume.

* Empty if there is no path (the root volume is not part of the path stack, nor
  is the leading `\` as this is handled by the `IsPinned` property)
* The last element of `PathStack` is `string.Empty`, if there should be a
  trailing `\`, which is equivalent to the current directory.

When joining the path:

* If not UNC:
  * `Path = RootVolume + IsPinned ? "\" : "" + Join("\", PathStack)`
* Else
  * `Path = Join("\", { RootVolume, PathStack })`
* (the last argument is to be read as a new list, with `RootVolume` at the start)

## Unix Path

Unix paths are much simpler than Windows paths.

| Path    | RootVolume | IsPinned | PathStack |
|---------|------------|----------|-----------|
| `""`    | `""`       | false    | [0]       |
| `/`     | `""`       | true     | ""        |
| `foo`   | `""`       | false    | foo       |
| `foo/`  | `""`       | false    | foo, ""   |
| `/foo`  | `""`       | true     | foo       |
| `/foo/` | `""`       | true     | foo, ""   |

* `RootVolume` is always empty, as Unix is either pinned (`/`) or relative.
* `IsPinned` indicates if the path starts from the root `/`. Else it is
  relative.

The `PathStack` is the same as Windows. It is everything to the right of the
root volume (which is always empty), and so completely describes the path,
except if it is pinned or not.

* Empty if there is no path (the root volume is not part of the path stack, nor
  is the leading `/` as this is handled by the `IsPinned` property)
* The last element of `PathStack` is `string.Empty`, if there should be a
  trailing `/`, which is equivalent to the current directory.

When joining the path, is the same as Windows, and there is no concept of UNC
paths for Unix paths, so this option doesn't apply.

* `Path = Join("\", { RootVolume, PathStack })`
  * (the last argument is to be read as a new list, with `RootVolume` at the start)

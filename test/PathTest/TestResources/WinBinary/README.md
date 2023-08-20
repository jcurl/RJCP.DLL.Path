# WinBinary Test Resources

These resources are for testing executable binary identification. They contain
snippets from valid binaries, as well as potential errors to check for out of
bounds access.

Here's a high level overview of the [PE
Format](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format). There
are a number of resources, e.g.
[0xrick](https://0xrick.github.io/win-internals/pe3/), etc.

The following file samples are obtained by binaries from the Internet (most from
Microsoft) as part of their installers or Operating System. They don't contain
any code in of themselves (and so are not executable), containing only
sufficient PE headers for detection.

| File                     | Target OS | Machine Type | IsExe | IsDll |  LE   | SubSystem | Arch  | Word  | Notes                              |
| :----------------------- | :-------- | :----------- | :---: | :---: | :---: | :-------- | :---: | :---: | :--------------------------------- |
| win32cui_a014c_exe.bin   | Windows   | Intel 386    |   X   |       |   X   | Console   |  32   |  32   |                                    |
| win32gui_a014c_exe.bin   | Windows   | Intel 386    |   X   |       |   X   | GUI       |  32   |  32   |                                    |
| win32gui_a014c_dll.bin   | Windows   | Intel 386    |       |   X   |   X   | GUI       |  32   |  32   |                                    |
| win32nat_a014c_sys.bin   | Windows   | Intel 386    |       |       |   X   | Native    |  32   |  32   |                                    |
| win32pos_a014c_exe.bin   | Windows   | Intel 386    |   X   |       |   X   | Posix     |  32   |  32   | From Interix 3.5 setup ISO         |
| win32pos_a014c_dll.bin   | Windows   | Intel 386    |   X   |       |   X   | Posix     |  32   |  64   | From Interix 3.5 setup ISO libm.so |
| win95cui_a014c_exe.bin   | Windows   | Intel 386    |   X   |       |   X   | Console   |  32   |  32   | Windows 95                         |
| win95gui_a014c_exe.bin   | Windows   | Intel 386    |   X   |       |   X   | GUI       |  32   |  32   | Windows 95                         |
| win95gui_a014c_dll.bin   | Windows   | Intel 386    |       |   X   |   X   | GUI       |  32   |  32   | Windows 95                         |
| win32cui_a8664_exe.bin   | Windows   | AMD x86_64   |   X   |       |   X   | Console   |  64   |  64   |                                    |
| win32gui_a8664_exe.bin   | Windows   | AMD x86_64   |   X   |       |   X   | GUI       |  64   |  64   |                                    |
| win32gui_a8664_dll.bin   | Windows   | AMD x86_64   |       |   X   |   X   | GUI       |  64   |  64   |                                    |
| win32nat_a8664_sys.bin   | Windows   | AMD x86_64   |       |       |   X   | Native    |  64   |  64   |                                    |
| win32cui_a01c4_exe.bin   | Windows   | ARMNT 32-bit |   X   |       |   X   | Console   |  32   |  32   | From Surface RT                    |
| win32gui_a01c4_exe.bin   | Windows   | ARMNT 32-bit |   X   |       |   X   | GUI       |  32   |  32   | From Surface RT                    |
| win32gui_a01c4_dll.bin   | Windows   | ARMNT 32-bit |       |   X   |   X   | GUI       |  32   |  32   | From Surface RT                    |
| win32nat_a01c4_sys.bin   | Windows   | ARMNT 32-bit |       |       |   X   | Native    |  32   |  32   | From Surface RT                    |
| win32cui_aaa64_exe.bin   | Windows   | AArch 64 ARM |   X   |       |   X   | Console   |  64   |  64   |                                    |
| win32gui_aaa64_exe.bin   | Windows   | AArch 64 ARM |   X   |       |   X   | GUI       |  64   |  64   |                                    |
| win32gui_aaa64_dll.bin   | Windows   | AArch 64 ARM |       |   X   |   X   | GUI       |  64   |  64   |                                    |
| win32nat_aaa64_sys.bin   | Windows   | AArch 64 ARM |       |       |   X   | Native    |  64   |  64   |                                    |
| win32cui_a0200_exe32.bin | Windows   | Itanium 64   |   X   |       |   X   | Console   |  64   |  32   | From WinXP IA64 installer disk     |
| win32gui_a0200_exe32.bin | Windows   | Itanium 64   |   X   |       |   X   | GUI       |  64   |  32   | From WinXP IA64 installer disk     |
| win32cui_a0200_dll32.bin | Windows   | Itanium 64   |       |   X   |   X   | Console   |  64   |  32   | From WinXP IA64 installer disk     |
| win32nat_a0200_sys32.bin | Windows   | Itanium 64   |       |       |   X   | Native    |  64   |  32   | From WinXP IA64 installer disk     |
| win32cui_a0200_exe.bin   | Windows   | Itanium 64   |   X   |       |   X   | Console   |  64   |  64   | From Win2003 SP1                   |
| win32gui_a0200_exe.bin   | Windows   | Itanium 64   |   X   |       |   X   | GUI       |  64   |  64   | From Win2003 SP1                   |
| win32gui_a0200_dll.bin   | Windows   | Itanium 64   |       |   X   |   X   | GUI       |  64   |  64   | From Win2003 SP1                   |
| win32nat_a0200_sys.bin   | Windows   | Itanium 64   |       |       |   X   | Native    |  64   |  64   | From Win2003 SP1 SYS driver        |
| win32cui_a0184_exe.bin   | Windows   | Alpha        |   X   |       |   X   | Console   |  32   |  32   | From Win2000                       |
| win32gui_a0184_exe.bin   | Windows   | Alpha        |   X   |       |   X   | GUI       |  32   |  32   | From Win2000                       |
| win32cui_a0184_dll.bin   | Windows   | Alpha        |       |   X   |   X   | Console   |  32   |  32   | From Win2000                       |
| win32nat_a0184_sys.bin   | Windows   | Alpha        |       |   X   |   X   | Native    |  32   |  32   | From Win2000 SYS driver            |

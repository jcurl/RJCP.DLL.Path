# FileInfoCheck

This directory contains a collection of binaries that can be used to test
different the `FileExecutable` class implementation. This is done as an
integration test, and not as unit tests, as the binaries required for testing
may be licensed and therefore can't be redistributed in the repository.

## Execution of Test Cases

To run the test cases build the tool `FileInfoCheck`, and then run it in the
current directory with the command:

```sh
FileInfoCheck FileexecutableTest.xml
```

It will output the results, if the tests pass or fail. e.g.

```sh
$ FileInfoCheck FileExecutableTest.xml
File: riscv64/bash (PASSED)
File: riscv64/ldconfig (PASSED)
File: riscv64/libpopt.so.0.0.0 (FAILED)
      Machine:     unknown      Unknown
      TargetOs:    unknown      Unknown
      Is LE:       true         True
      Is Exe:      false        False
   -> Is DLL:      true         False
      Is Core:     false        False
      Is PIE:      true         True
      Arch Size:   64           64
File: riscv64/libc-2.27.so (FAILED)
      Machine:     unknown      Unknown
      TargetOs:    unknown      Unknown
      Is LE:       true         True
      Is Exe:      true         True
   -> Is DLL:      true         False
      Is Core:     false        False
      Is PIE:      true         True
      Arch Size:   64           64
```

## Test Cases and Special Notes

For information about the expected properties for each file, just open the
`FileExecutableTest.xml` file and read the expected settings. The following
Table contains what is interesting about the file.

| File                   | Notes                                                 |
| :--------------------- | :---------------------------------------------------- |
| riscv/bash             | It's a dynamic executable (PIE), should have P_INTERP |
| riscv/ldconfig         | It's an executable, and doesn't have a P_INTERP       |
| riscv/libpopt.so.0.0.0 |                                                       |
| risv/libc-2.27.so      | It's primary a DLL with P_INTERP                      |

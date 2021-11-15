namespace RJCP.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents a parsed Window's path.
    /// </summary>
    public sealed class WindowsPath : Path
    {
        // The number of times to iterate a parent path (..) at the beginning of the stack. This is an optimization that
        // can speed up appending operations.
        private int m_Parents;

        private WindowsPath() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPath"/> class.
        /// </summary>
        /// <param name="path">The path that should be parsed and normalized.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is a DOS path, but the drive letter is invalid.
        /// <para>- or -</para>
        /// <paramref name="path"/> is a UNC path, but is too short, or has an invalid UNC character.
        /// <para>- or -</para>
        /// When normalizing <paramref name="path"/>, the parent directory is on the root volume (e.g. <c>C:\..</c>).
        /// </exception>
        public WindowsPath(string path)
        {
            ParsePath(path);
            Check();
        }

        [Conditional("DEBUG")]
        private void Check()
        {
            // Check consistency rules for the path, to ensure that any changes to this code doesn't cause regressions.
            // This is not done in RELEASE mode for performance.

            if (IsPinned) {
                if (m_Parents != 0)
                    throw new InvalidOperationException($"Inconsistent state when pinned - m_Parent = {m_Parents}");
            } else {
                int l = 0;
                for (int i = 0; i < PathStack.Count; i++) {
                    if (string.CompareOrdinal("..", PathStack[i]) == 0) l++;
                }
                if (m_Parents != l)
                    throw new InvalidOperationException($"Inconsistent state - m_Parent = {m_Parents}; counted {l}");
            }

            if (m_Parents > PathStack.Count)
                throw new InvalidOperationException($"Inconsistent state - m_Parent {m_Parents} more than count {PathStack.Count}");
        }

        // After ParsePath is finished, the properties are set:
        // - IsDos - Has a DOS drive letter, like C:
        // - IsUnc - Has a UNC path, like \\server or \\server\share, including NT paths \\.\COM10
        // - IsPinned - The path is rooted (always for UNC, and for DOS C:\)
        // - RootVolume - The drive letter (C:) or UNC root (\\server\share)

        // The m_PathStack is everything to the right of the root volume.
        // - Empty if there is no path (the root volume is not part of the path stack, nor is the leading '\' as this is
        //   handled by the IsPinned property)
        // - The last element of m_PathStack is string.Empty, if there should be a trailing '\', which is equivalent to
        //   the current directory.

        // For example:
        //
        // PathStack: [0] indicates it is length zero; else it contains the list
        //
        // | Path          | RootVolume | IsPinned | IsUnc | IsDos | PathStack |
        // |---------------|------------|----------|-------|-------|-----------|
        // | ""            | ""         | false    | false | false | [0]       |
        // | \             | ""         | true     | false | false | ""        |
        // | C:            | C:         | false    | false | true  | [0]       |
        // | C:\           | C:         | true     | false | true  | ""        |
        // | foo           | ""         | false    | false | false | foo       |
        // | foo\          | ""         | false    | false | false | foo, ""   |
        // | C:foo         | C:         | false    | false | true  | foo       |
        // | C:\foo        | C:         | true     | false | true  | foo       |
        // | C:\foo\       | C:         | true     | false | true  | foo, ""   |
        // | \\srv         | \\srv      | true     | true  | false | [0]       |
        // | \\srv\s       | \\srv\s    | true     | true  | false | [0]       |
        // | \\srv\s\      | \\srv\s    | true     | true  | false | ""        |
        // | \\srv\s\foo   | \\srv\s    | true     | true  | false | foo       |
        // | \\srv\s\foo\  | \\srv\s    | true     | true  | false | foo, ""   |
        // | \\srv\s\foo\b | \\srv\s    | true     | true  | false | foo, b    |

        // If the path ends with a "\", it is equivalent to "\." and the last element on the path stack is then an empty
        // string.

        // Rules for combining a DOS path is different to UNC paths. The main reason is that Windows drive letters are
        // references to a current working directory, and on their own, aren't a path, unless combined with something
        // else at the end. That makes UNC paths always pinned.
        //
        // If not UNC:
        // - Path = RootVolume + IsPinned ? "\" : "" + Join("\", PathStack)
        //
        // If UNC
        // - Path = Join("\", { RootVolume, PathStack })
        //   (the last argument is to be read as a new list, with RootVolume at the start)

        private void ParsePath(string path)
        {
            if (path == null) {
                RootVolume = string.Empty;
                PathStack.InitializeEmpty();
                m_Path = string.Empty;
                return;
            }

            string trimmedPath = path.Trim();
            if (string.IsNullOrEmpty(trimmedPath)) {
                RootVolume = string.Empty;
                PathStack.InitializeEmpty();
                m_Path = string.Empty;
                return;
            }

            if (trimmedPath.Length == 1) {
                RootVolume = string.Empty;
                if (IsDirSepChar(trimmedPath[0])) {
                    // The '\' is the root, pinned, but no root volume.
                    IsPinned = true;
                    PathStack.InitializeRoot();
                    m_Path = @"\";
                } else if (trimmedPath[0] == '.') {
                    PathStack.InitializeEmpty();
                    m_Path = string.Empty;
                } else {
                    // The single character is a relative path.
                    PathStack.Initialize(trimmedPath);
                    m_Path = trimmedPath;
                }
                return;
            }

            // Where the unpinned path starts after finding the DOS letter or UNC path.
            int pathStart = CheckIsDos(trimmedPath);
            if (pathStart == 0) pathStart = CheckIsUnc(trimmedPath);

            if (RootVolume == null) RootVolume = string.Empty;
            if (pathStart == trimmedPath.Length) {
                PathStack.InitializeEmpty();
                return;
            }

            // From here is the remaining path, possibly starting with the '\' if it's pinned,
            if (IsDirSepChar(trimmedPath[pathStart])) {
                pathStart++;
                IsPinned = true;
                if (trimmedPath.Length == pathStart) {
                    PathStack.InitializeRoot();
                    return;
                }
            }

            List<string> stack = new List<string>();
            int ps = pathStart;
            for (int i = ps; i < trimmedPath.Length; i++) {
                char c = trimmedPath[i];
                if (IsDirSepChar(c)) {
                    if (i > ps) stack.Add(trimmedPath.Substring(ps, i - ps));
                    ps = i + 1;
                }
            }
            if (ps < trimmedPath.Length) {
                stack.Add(trimmedPath.Substring(ps));
            } else {
                stack.Add(string.Empty);
            }

            NormalizePath(stack);
        }

        private static readonly string[] DriveLetter = {
            "A:", "B:", "C:", "D:", "E:", "F:", "G:", "H:", "I:", "J:", "K:", "L:", "M:",
            "N:", "O:", "P:", "Q:", "R:", "S:", "T:", "U:", "V:", "W:", "X:", "Y:", "Z:"
        };

        private int CheckIsDos(string path)
        {
            if (path[1] == ':') {
                // Check for DOS X:
                char d = path[0];
                if (d >= 'a' && d <= 'z') {
                    RootVolume = DriveLetter[d - 'a'];
                } else if (d >= 'A' && d <= 'Z') {
                    RootVolume = DriveLetter[d - 'A'];
                } else {
                    throw new ArgumentException("Invalid path", nameof(path));
                }
                IsDos = true;
                return 2;
            }
            return 0;
        }

        private int CheckIsUnc(string path)
        {
            if (IsDirSepChar(path[0]) && IsDirSepChar(path[1])) {
                if (path.Length == 2)
                    throw new ArgumentException("Invalid UNC path", nameof(path));

                // Check for UNC \\server\share paths.
                int s = 2;
                for (int p = 2; p < path.Length; p++) {
                    char c = path[p];
                    if (IsDirSepChar(c)) {
                        if (p == s) {
                            // The 's' ensures that there is at least one character after the last '\' character.
                            throw new ArgumentException("Invalid UNC path", nameof(path));
                        }
                        if (s > 2) {
                            // We've found the UNC path
                            RootVolume = string.Format(@"\\{0}\{1}",
                                path.Substring(2, s - 3),
                                path.Substring(s, p - s));
                            IsUnc = true;
                            IsPinned = true;
                            return p;
                        } else {
                            s = p + 1;
                        }
                    }
                }

                if (!IsUnc) {
                    // We didn't find the final '\' separator, so this is the root volume
                    if (s == 2) {
                        RootVolume = string.Format(@"\\{0}", path.Substring(2));
                    } else {
                        RootVolume = string.Format(@"\\{0}\{1}",
                            path.Substring(2, s - 3),
                            path.Substring(s));
                    }
                    IsPinned = true;
                    IsUnc = true;
                    return path.Length;
                }
            }
            return 0;
        }

        private static bool IsDirSepChar(char c)
        {
            return c == '/' || c == '\\';
        }

        private void NormalizePath(List<string> stack)
        {
            // Normalize the path, so that . and .. are removed where possible
            // . -> Remove the current element
            // .. -> Remove this element and the previous element, if the previous element exists and not pinned

            int i = 0;
            int l = 0;
            int c = stack.Count;
            while (i < c) {
                string node = stack[i];
                if (string.CompareOrdinal(".", node) == 0) {
                    if (IsPinned && c == 1) {
                        PathStack.InitializeRoot();
                        return;
                    } else {
                        stack.RemoveAt(i);
                        --c;
                    }
                } else if (string.CompareOrdinal("..", node) == 0) {
                    if (i == l) {
                        if (IsPinned)
                            throw new ArgumentException("Invalid path when normalizing");
                        l++;
                        i++;
                    } else {
                        stack.RemoveAt(i);
                        stack.RemoveAt(i - 1);
                        c -= 2;
                        --i;
                        if (IsPinned && c == 0) {
                            if (IsUnc) {
                                PathStack.InitializeEmpty();
                            } else {
                                PathStack.InitializeRoot();
                            }
                            return;
                        }
                    }
                } else {
                    i++;
                }
            }
            PathStack.Initialize(stack);
            m_Parents = l;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a UNC style path.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if this instance is a UNC path (Universal Naming Convention); otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsUnc { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is DOS style path.
        /// </summary>
        /// <value>
        /// Returns <see langword="true"/> if this instance is a DOS style path; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsDos { get; private set; }

        /// <summary>
        /// Creates the path. To be implemented by the classes deriving from this abstract class.
        /// </summary>
        /// <param name="path">The path that is to be parsed.</param>
        /// <returns>
        /// A parsed path, given as a <see cref="Path"/> object, specific for the Operating System this class
        /// represents.
        /// </returns>
        protected override Path CreatePath(string path)
        {
            return new WindowsPath(path);
        }

        /// <summary>
        /// Appends the specified path to the current path.
        /// </summary>
        /// <param name="path">The path object to append to this object.</param>
        /// <returns>The appended <see cref="Path"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Can't append unpinned paths with different root volumes.
        /// </exception>
        /// <remarks>
        /// Appends the <paramref name="path"/> to this object and returns a new object if there is a change.
        /// <para>Appending a new path has the following rules.</para>
        /// <list type="bullet">
        /// <item>
        /// If the <paramref name="path"/> to append has no <see cref="Path.RootVolume"/> and is not
        /// <see cref="Path.IsPinned"/>, then the result is this path, plus the new path.
        /// </item>
        /// <item>
        /// If the <paramref name="path"/> is <see cref="IsDos"/>, both must have the same <see cref="Path.RootVolume"/>
        /// to be appended. If <paramref name="path"/> is <see cref="Path.IsPinned"/>, then the
        /// </item>
        /// </list>
        /// </remarks>
        public override Path Append(Path path)
        {
            // Both must be a Windows path
            if (!(path is WindowsPath winPath)) return this;

            // Rule 1
            if (IsEmpty(winPath)) return this;
            if (IsEmpty(this)) return path;

            if (winPath.IsPinned) {
                if (!string.IsNullOrEmpty(winPath.RootVolume)) return path; // Rule 2
                if (string.IsNullOrEmpty(RootVolume)) return path;          // Rule 4
            }

            // Rule 3
            if (!string.IsNullOrEmpty(RootVolume) &&
                !winPath.IsPinned && !string.IsNullOrEmpty(winPath.RootVolume) &&
                !winPath.RootVolume.Equals(RootVolume, StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException("Can't append unpinned paths with different root volumes", nameof(path));
            }

            WindowsPath newPath = new WindowsPath {
                IsDos = IsDos || winPath.IsDos,
                IsUnc = IsUnc || winPath.IsUnc,
                IsPinned = IsPinned || winPath.IsPinned,
                RootVolume = string.IsNullOrEmpty(RootVolume) ? winPath.RootVolume : RootVolume
            };

            if (winPath.IsPinned) {
                newPath.PathStack.Initialize(winPath.PathStack);
                newPath.Check();
                return newPath;
            }

            int leftCount = PathStack.Count;
            List<string> stack;
            int skip = 0;

            if (leftCount == 0) {
                stack = new List<string>();
            } else {
                bool leftTrim = string.IsNullOrEmpty(PathStack[leftCount - 1]) &&
                    winPath.PathStack.Count > 0;
                if (leftTrim) leftCount--;

                if (newPath.IsPinned && winPath.m_Parents > leftCount)
                    throw new ArgumentException("Invalid path when normalizing");

                if (IsPinned) {
                    if (leftCount == winPath.m_Parents && winPath.m_Parents == winPath.PathStack.Count) {
                        if (IsUnc) {
                            newPath.PathStack.InitializeEmpty();
                        } else {
                            newPath.PathStack.InitializeRoot();
                        }
                        newPath.Check();
                        return newPath;
                    }
                }

                // Don't copy node paths, that we'll remove again later by normalization.
                if (winPath.m_Parents > 0) {
                    skip = Math.Min(leftCount - m_Parents, winPath.m_Parents);
                }

                if (!leftTrim && skip == 0) {
                    stack = new List<string>(PathStack.Stack);
                    newPath.m_Parents = m_Parents;
                } else {
                    stack = new List<string>();
                    for (int i = 0; i < leftCount - skip; i++) {
                        stack.Add(PathStack[i]);
                    }
                    newPath.m_Parents = Math.Min(m_Parents, leftCount - skip);
                }
            }

            if (skip == 0) {
                stack.AddRange(winPath.PathStack.Stack);
                newPath.m_Parents += winPath.m_Parents;
            } else {
                for (int i = skip; i < winPath.PathStack.Count; i++) {
                    stack.Add(winPath.PathStack[i]);
                }
                newPath.m_Parents += Math.Max(winPath.m_Parents - skip, 0);
            }
            newPath.PathStack.Initialize(stack);
            newPath.Check();
            return newPath;
        }

        private static bool IsEmpty(WindowsPath path)
        {
            return string.IsNullOrEmpty(path.RootVolume) && path.PathStack.Count == 0;
        }

        /// <summary>
        /// Gets the parent of the current path.
        /// </summary>
        /// <returns>The new parent of this path, or the root volume.</returns>
        public override Path GetParent()
        {
            if (IsPinned) {
                if (PathStack.Count == 0) return this;
                if (!IsUnc && PathStack.Count == 1 && string.IsNullOrEmpty(PathStack[0])) return this;
            }
            if (PathStack.Count == 0 && IsPinned) return this;

            bool appendParent = false;
            int nodes = PathStack.Count;
            if (nodes > 0 && string.IsNullOrEmpty(PathStack[nodes - 1])) nodes--;

            WindowsPath newPath = new WindowsPath {
                IsDos = IsDos,
                IsUnc = IsUnc,
                IsPinned = IsPinned,
                RootVolume = RootVolume
            };

            if (IsPinned) {
                if (nodes == 1) {
                    if (IsUnc) {
                        newPath.PathStack.InitializeEmpty();
                    } else {
                        newPath.PathStack.InitializeRoot();
                    }
                    return newPath;
                } else if (nodes > 1) {
                    nodes--;
                }
            } else {
                if (nodes > m_Parents) {
                    nodes--;
                } else {
                    appendParent = true;
                }
            }

            if (nodes == 0) {
                if (appendParent) {
                    newPath.PathStack.Initialize("..");
                    newPath.m_Parents = 1;
                } else {
                    newPath.PathStack.InitializeEmpty();
                }
            } else {
                List<string> stack = new List<string>();
                for (int i = 0; i < nodes; i++) {
                    stack.Add(PathStack[i]);
                }
                if (appendParent) {
                    stack.Add("..");
                    newPath.m_Parents = m_Parents + 1;
                } else {
                    newPath.m_Parents = m_Parents;
                }
                newPath.PathStack.Initialize(stack);
            }
            newPath.Check();
            return newPath;
        }

        /// <summary>
        /// Gets the relative path given a base path.
        /// </summary>
        /// <param name="basePath">The base path, which will be used to get the relative path.</param>
        /// <returns>
        /// A new <see cref="Path"/> that is relative to the <paramref name="basePath"/> that returns this path if
        /// appended. i.e. it returns the path that must be applied to the <paramref name="basePath"/> to get this path.
        /// The result is trimmed. Comparisons are done case insensitive.
        /// </returns>
        public override Path GetRelative(Path basePath)
        {
            // Both must be a Windows path
            if (!(basePath is WindowsPath winPath)) return this;
            return GetRelative(winPath, false);
        }

        /// <summary>
        /// Gets the relative path given a base path.
        /// </summary>
        /// <param name="basePath">The base path, which will be used to get the relative path.</param>
        /// <param name="caseSensitive">
        /// Set to <see langword="true"/> if the comparison should be done case sensitive. Some Windows file systems are
        /// case sensitive, but most are not.
        /// </param>
        /// <returns>
        /// A new <see cref="Path"/> that is relative to the <paramref name="basePath"/> that returns this path if
        /// appended. i.e. it returns the path that must be applied to the <paramref name="basePath"/> to get this path.
        /// The result is trimmed. Comparisons are done case sensitive as per <paramref name="caseSensitive"/>. In all
        /// cases, the root volume (the drive letter, or UNC path) is always compared with case insensitive.
        /// </returns>
        public Path GetRelative(WindowsPath basePath, bool caseSensitive)
        {
            // If the root volumes differ, then they cannot be compared. Return this path, as there is no relative path
            // to this path. Even if they're both UNC and equivalent (e.g. \\srv\ and \\srv), then we can still return
            // this object, making for a fast check.
            if (!RootVolume.Equals(basePath.RootVolume, StringComparison.OrdinalIgnoreCase)) return this;

            // If both are pinned, or both are unpinned, we can calculate the relative difference between the two. Else
            // return the current object.
            if (IsPinned != basePath.IsPinned) return this;

            // | this   | base   | result | Notes                    |
            // |--------|--------|--------|--------------------------|
            // | X:\foo | X:\    | foo    | X:\ + foo = X:\foo       |
            // | X:\    | X:\foo | ..     | X:\foo + .. = X:\        |
            // | X:\foo | X:\bar | ..\foo | X:\bar + ..\foo = X:\foo |
            // | X:\foo | Y:\bar | X:\foo | Y:\bar + X:\foo = X:\foo |

            int leftLen = PathStack.TrimmedLength();
            int rightLen = basePath.PathStack.TrimmedLength();

            int match = -1;
            int pos = m_Parents < basePath.m_Parents ? m_Parents : basePath.m_Parents;

            if (caseSensitive) {
                while (pos < leftLen && pos < rightLen) {
                    string left = PathStack[pos];
                    string right = basePath.PathStack[pos];

                    if (string.CompareOrdinal(left, right) != 0) {
                        match = pos;
                        break;
                    }
                    pos++;
                }
            } else {
                while (pos < leftLen && pos < rightLen) {
                    string left = PathStack[pos];
                    string right = basePath.PathStack[pos];

                    if (!left.Equals(right, StringComparison.OrdinalIgnoreCase)) {
                        match = pos;
                        break;
                    }
                    pos++;
                }
            }

            if (match == -1) match = pos;      // The length of the shortest stack

            WindowsPath newPath = new WindowsPath {
                IsDos = false,
                IsUnc = false,
                IsPinned = false,
                RootVolume = string.Empty
            };

            List<string> stack = new List<string>();
            newPath.m_Parents = rightLen - match;
            for (int i = 0; i < newPath.m_Parents; i++) {
                stack.Add("..");
            }

            for (int i = match; i < leftLen; i++) {
                stack.Add(PathStack[i]);
            }
            newPath.PathStack.Initialize(stack);
            newPath.Check();
            return newPath;
        }

        /// <summary>
        /// Trims the trailing folder character, if one exists.
        /// </summary>
        /// <returns>A new <see cref="Path"/> object with the trailing path trimmed.</returns>
        /// <remarks>
        /// If the path ends with a directory separator character, it is removed, unless it would make the path from
        /// being an absolute path, to a relative path, in which case it is not removed.
        /// </remarks>
        public override Path Trim()
        {
            if (IsTrimmed()) return this;

            WindowsPath newPath = new WindowsPath {
                IsDos = IsDos,
                IsUnc = IsUnc,
                IsPinned = IsPinned,
                RootVolume = RootVolume
            };

            if (PathStack.Count == 1) {
                newPath.PathStack.InitializeEmpty();
            } else {
                List<string> stack = new List<string>();
                for (int i = 0; i < PathStack.Count - 1; i++) {
                    stack.Add(PathStack[i]);
                }
                newPath.PathStack.Initialize(stack);
                newPath.m_Parents = m_Parents;
            }
            newPath.Check();
            return newPath;
        }

        /// <summary>
        /// Determines whether this instance is trimmed.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if this path is trimmed; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool IsTrimmed()
        {
            if (PathStack.Count == 0) return true;
            if (IsPinned && !IsUnc && PathStack.Count == 1) return true;
            if (!string.IsNullOrEmpty(PathStack[PathStack.Count - 1]))
                return true;

            return false;
        }

        private string m_Path;

        /// <summary>
        /// Returns the path as a <see cref="string"/>.
        /// </summary>
        /// <returns>The path as a <see cref="string"/>.</returns>
        public override string ToString()
        {
#if !DEBUG
            // Only use this when not debugging to improve performance. Else using the debugger may result in some
            // cached behaviour while parsing this object.
            if (m_Path != null) return m_Path;
#endif

            StringBuilder path = new StringBuilder();
            path.Append(RootVolume);
            if (PathStack.Count > 0) {
                if (IsPinned) path.Append('\\');

                if (PathStack.Count == 1) {
                    path.Append(PathStack[0]);
                } else if (PathStack.Count > 1) {
                    path.Append(string.Join(@"\", PathStack.Stack));
                }
            }

            m_Path = path.ToString();
            return m_Path;
        }
    }
}

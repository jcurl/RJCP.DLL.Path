namespace RJCP.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents a parsed Posix path.
    /// </summary>
    public sealed class UnixPath : Path
    {
        // The number of times to iterate a parent path (..) at the beginning of the stack. This is an optimization that
        // can speed up appending operations.
        private int m_Parents;

        private UnixPath()
        {
            RootVolume = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixPath"/> class.
        /// </summary>
        /// <param name="path">The path that should be parsed and normalized.</param>
        /// <exception cref="ArgumentException">
        /// When normalizing <paramref name="path"/>, the parent directory is on the root volume (e.g. <c>\..</c>).
        /// </exception>
        public UnixPath(string path) : this()
        {
            ParsePath(path);
            Check();
        }

        [Conditional("DEBUG")]
        private void Check()
        {
            // Check consistency rules for the path, to ensure that any changes to this code doesn't cause regressions.
            // This is not done in RELEASE mode for performance.
            if (RootVolume is null)
                throw new InvalidOperationException("Inconsistent state - RootVolume is null");

            if (!string.IsNullOrEmpty(RootVolume))
                throw new InvalidOperationException($"Inconsistent state - RootVolume = {RootVolume}");

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

        // See docs/PathStructure.md on how this is constructed.

        private void ParsePath(string path)
        {
            if (path is null) {
                PathStack.InitializeEmpty();
                m_Path = string.Empty;
                return;
            }

            string trimmedPath = path.Trim();
            if (string.IsNullOrEmpty(trimmedPath)) {
                PathStack.InitializeEmpty();
                m_Path = string.Empty;
                return;
            }

            if (trimmedPath.Length == 1) {
                if (IsDirSepChar(trimmedPath[0])) {
                    // The '/' is the root, pinned, but no root volume.
                    IsPinned = true;
                    PathStack.InitializeRoot();
                    m_Path = "/";
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
            int pathStart = 0;

            // From here is the remaining path, possibly starting with the '\' if it's pinned,
            if (IsDirSepChar(trimmedPath[0])) {
                IsPinned = true;
                if (trimmedPath.Length == 1) {
                    PathStack.InitializeRoot();
                    return;
                }
                pathStart = 1;
            }

            List<string> stack = new();
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

        private static bool IsDirSepChar(char c)
        {
            return c == '/';
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
                            PathStack.InitializeRoot();
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
        /// Creates the path. To be implemented by the classes deriving from this abstract class.
        /// </summary>
        /// <param name="path">The path that is to be parsed.</param>
        /// <returns>
        /// A parsed path, given as a <see cref="Path"/> object, specific for the Operating System this class
        /// represents.
        /// </returns>
        protected override Path CreatePath(string path)
        {
            return new UnixPath(path);
        }

        /// <summary>
        /// Appends the specified path to the current path.
        /// </summary>
        /// <param name="path">The path object to append to this object.</param>
        /// <returns>The appended <see cref="Path"/>.</returns>
        /// <exception cref="ArgumentException">Invalid path when normalizing.</exception>
        /// <remarks>
        /// Appends the <paramref name="path"/> to this object and returns a new object if there is a change.
        /// <para>Appending a new path has the following rules.</para>
        /// </remarks>
        public override Path Append(Path path)
        {
            // Both must be a Unix path
            if (path is not UnixPath unixPath) return this;

            // Rule 1
            if (IsEmpty(unixPath)) return this;
            if (IsEmpty(this)) return path;

            // Rule 4
            if (unixPath.IsPinned) return path;

            UnixPath newPath = new() {
                IsPinned = IsPinned || unixPath.IsPinned,
            };

            int leftCount = PathStack.Count;
            List<string> stack;
            int skip = 0;

            if (leftCount == 0) {
                newPath.PathStack.Initialize(unixPath.PathStack);
                newPath.m_Parents = unixPath.m_Parents;
            } else {
                bool leftTrim = string.IsNullOrEmpty(PathStack[leftCount - 1]) &&
                    unixPath.PathStack.Count > 0;
                if (leftTrim) leftCount--;

                if (newPath.IsPinned && unixPath.m_Parents > leftCount)
                    throw new ArgumentException("Invalid path when normalizing");

                if (IsPinned) {
                    if (leftCount == unixPath.m_Parents && unixPath.m_Parents == unixPath.PathStack.Count) {
                        newPath.PathStack.InitializeRoot();
                        newPath.Check();
                        return newPath;
                    }
                }

                // Don't copy node paths, that we'll remove again later by normalization.
                if (unixPath.m_Parents > 0) {
                    skip = Math.Min(leftCount - m_Parents, unixPath.m_Parents);
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

                if (skip == 0) {
                    stack.AddRange(unixPath.PathStack.Stack);
                    newPath.m_Parents += unixPath.m_Parents;
                } else {
                    for (int i = skip; i < unixPath.PathStack.Count; i++) {
                        stack.Add(unixPath.PathStack[i]);
                    }
                    newPath.m_Parents += Math.Max(unixPath.m_Parents - skip, 0);
                }
                newPath.PathStack.Initialize(stack);
            }
            newPath.Check();
            return newPath;
        }

        private static bool IsEmpty(UnixPath path)
        {
            return path.PathStack.Count == 0;
        }

        /// <summary>
        /// Gets the parent of the current path.
        /// </summary>
        /// <returns>The new parent of this path, or the root volume.</returns>
        public override Path GetParent()
        {
            if (IsPinned) {
                if (PathStack.Count == 0) return this;
                if (PathStack.Count == 1 && string.IsNullOrEmpty(PathStack[0])) return this;
            }

            bool appendParent = false;
            int nodes = PathStack.Count;
            if (nodes > 0 && string.IsNullOrEmpty(PathStack[nodes - 1])) nodes--;

            UnixPath newPath = new() {
                IsPinned = IsPinned,
            };

            if (IsPinned) {
                if (nodes == 1) {
                    newPath.PathStack.InitializeRoot();
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
                List<string> stack = new();
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
            if (basePath is not UnixPath winPath) return this;
            return GetRelativeUnix(winPath);
        }

        private UnixPath GetRelativeUnix(UnixPath basePath)
        {
            // If both are pinned, or both are unpinned, we can calculate the relative difference between the two. Else
            // return the current object.
            if (IsPinned != basePath.IsPinned) return this;

            // | this | base | result | Notes                |
            // |------|------|--------|----------------------|
            // | \foo | \    | foo    | \ + foo = \foo       |
            // | \    | \foo | ..     | \foo + .. = \        |
            // | \foo | \bar | ..\foo | \bar + ..\foo = \foo |

            int leftLen = PathStack.TrimmedLength();
            int rightLen = basePath.PathStack.TrimmedLength();

            int match = -1;
            int pos = m_Parents < basePath.m_Parents ? m_Parents : basePath.m_Parents;

            while (pos < leftLen && pos < rightLen) {
                string left = PathStack[pos];
                string right = basePath.PathStack[pos];

                if (string.CompareOrdinal(left, right) != 0) {
                    match = pos;
                    break;
                }
                pos++;
            }

            if (match == -1) match = pos;      // The length of the shortest stack

            UnixPath newPath = new() {
                IsPinned = false,
            };

            List<string> stack = new();
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

            UnixPath newPath = new() {
                IsPinned = IsPinned,
            };

            newPath.PathStack.InitializeTrimmed(PathStack);
            newPath.m_Parents = m_Parents;
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
            if (IsPinned && PathStack.Count == 1) return true;
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
            if (PathStack.Count == 0) {
                m_Path = string.Empty;
            } else {
                StringBuilder path = new();
                if (IsPinned) path.Append('/');
#if NET6_0_OR_GREATER
                path.AppendJoin('/', PathStack.Stack);
#else
                path.Append(string.Join("/", PathStack.Stack));
#endif
                m_Path = path.ToString();
            }

            return m_Path;
        }
    }
}

namespace RJCP.IO.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A Path stack, representing each path element.
    /// </summary>
    /// <remarks>
    /// This class is intended to only be used for classes deriving from <see cref="Path"/>, as it has almost no checks
    /// on the input to make it as fast as possible.
    /// </remarks>
    public class PathStack
    {
        private IList<string> m_PathStack;

        private static readonly IList<string> EmptyStack =
#if NETFRAMEWORK
            new ReadOnlyCollection<string>(new string[0]);
#else
            Array.Empty<string>();
#endif

        private static readonly IList<string> RootStack =
            new ReadOnlyCollection<string>(new string[] { string.Empty });

        /// <summary>
        /// Initializes the path stack to be empty, no node elements.
        /// </summary>
        public void InitializeEmpty()
        {
            m_PathStack = EmptyStack;
            Count = 0;
        }

        /// <summary>
        /// Initializes the path to be the root path.
        /// </summary>
        /// <remarks>The only element is an empty path element, indicating the leaf node is a directory.</remarks>
        public void InitializeRoot()
        {
            m_PathStack = RootStack;
            Count = 1;
        }

        /// <summary>
        /// Initializes the path to be a single node.
        /// </summary>
        /// <param name="root">The name of the root node.</param>
        public void Initialize(string root)
        {
            m_PathStack = new List<string>() { root };
            Count = 1;
        }

        /// <summary>
        /// Initializes the path stack to be the stack given.
        /// </summary>
        /// <param name="stack">The stack to initialize as.</param>
        /// <remarks>
        /// This stack is made as a reference copy only. The parameter <paramref name="stack"/> should not be modified
        /// after assigning to this property, else undefined behaviour can occur.
        /// </remarks>
        public void Initialize(IList<string> stack)
        {
            m_PathStack = stack;
            Count = stack.Count;
        }

        /// <summary>
        /// Initializes the specified stack from another <see cref="PathStack"/> object.
        /// </summary>
        /// <param name="stack">The stack to initialize.</param>
        /// <remarks>As path stacks are expected to be immutable, a reference only copy is made.</remarks>
        public void Initialize(PathStack stack)
        {
            m_PathStack = stack.m_PathStack;
            Count = stack.Count;
        }

        /// <summary>
        /// Initializes the specified stack from another <see cref="PathStack"/> removing the last element.
        /// </summary>
        /// <param name="stack">The stack to initialize from.</param>
        public void InitializeTrimmed(PathStack stack)
        {
            if (stack.Count == 1) {
                m_PathStack = EmptyStack;
                Count = 0;
            } else {
                List<string> newstack = new List<string>();
                for (int i = 0; i < stack.Count - 1; i++) {
                    newstack.Add(stack.m_PathStack[i]);
                }
                m_PathStack = newstack;
                Count = newstack.Count;
            }
        }

        /// <summary>
        /// Gets the number of elements (including the leaf element also if it's empty) in the path stack.
        /// </summary>
        /// <value>The number of elements (including the leaf element also if it's empty) in the path stack.</value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the path node <see cref="System.String"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The path node at the specified index.</returns>
        public string this[int index] { get { return m_PathStack[index]; } }

        /// <summary>
        /// Gets the read-only stack reference.
        /// </summary>
        /// <value>The stack reference.</value>
        public IEnumerable<string> Stack
        {
            get { return m_PathStack; }
        }

        /// <summary>
        /// Gets the number of path nodes, with the last node trimmed if it is empty.
        /// </summary>
        /// <returns>The number of path nodes.</returns>
        public int TrimmedLength()
        {
            if (Count == 0) return 0;
            if (string.IsNullOrEmpty(m_PathStack[Count - 1])) return Count - 1;
            return Count;
        }
    }
}

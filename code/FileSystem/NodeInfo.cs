// Required that the derived classes must implement the hash code. It is better that the override is there, so that the
// hash code can be made readonly as part of its constructor, proving immutability.

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace RJCP.IO.FileSystem
{
    /// <summary>
    /// A common implementation for equality for <see cref="INodeInfo"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object being implemented.</typeparam>
    /// <typeparam name="TE">
    /// The type of the extended information. This is downcasted to <see cref="IFileSystemExtended"/> through
    /// <see cref="NodeInfo{T, TE}.Extended"/>, returning the property <see cref="NodeInfo{T, TE}.ExtendedInfo"/> which
    /// is effectively aliased to avoid having to do excessive typecasting in implementations.
    /// </typeparam>
    /// <remarks>
    /// Ensure that derived classes implement <see cref="object.GetHashCode"/> to complement the implementation of
    /// <see cref="Equals(object)"/> in this abstract class.
    /// </remarks>
    internal abstract class NodeInfo<T, TE> : INodeInfo where T : NodeInfo<T, TE> where TE : IFileSystemExtended
    {
        /// <summary>
        /// Gets the type of information obtained, which depends on the OS at runtime.
        /// </summary>
        /// <value>The type of the information obtained.</value>
        public abstract NodeInfoType Type { get; }

        /// <summary>
        /// If this is a reparse point, get the path of the target.
        /// </summary>
        /// <value>The link target path.</value>
        /// <remarks>
        /// If this value is <see langword="null"/> or empty, then this is not a reparse point, or it is not known.
        /// </remarks>
        public abstract string LinkTarget { get; }

        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        /// <value>The path.</value>
        public abstract string Path { get; }

        /// <summary>
        /// Gets the extended information based on the implementing type.
        /// </summary>
        /// <value>The extended information.</value>
        /// <remarks>
        /// Derived classes from this abstract class should override and use this concrete type, to provide for better
        /// readable code. .NET C# doesn't support overriding methods with different return types.
        /// </remarks>
        protected abstract TE ExtendedInfo { get; }

        /// <summary>
        /// Gets the extended information, which the <see cref="FileSystemNodeInfo"/> can return.
        /// </summary>
        /// <value>The extended information.</value>
        public IFileSystemExtended Extended { get { return ExtendedInfo; } }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as INodeInfo);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(INodeInfo other)
        {
            if (other is null) return false;
            if (other.GetType() != GetType()) return false;
            return Equals((T)other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>An implementation should implement this method specific to the implementation obtained.</remarks>
        protected abstract bool Equals(T other);
    }
}

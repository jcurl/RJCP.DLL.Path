// Required that the derived classes must implement the hash code. It is better that the override is there, so that the
// hash code can be made readonly as part of its constructor, proving immutability.

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace RJCP.IO.FileSystem
{
    /// <summary>
    /// A common implementation for equality for <see cref="INodeInfo"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object being implemented.</typeparam>
    /// <remarks>
    /// Ensure that derived classes implement <see cref="object.GetHashCode"/> to complement the implementation of
    /// <see cref="Equals(object)"/> in this abstract class.
    /// </remarks>
    internal abstract class NodeInfo<T> : INodeInfo where T : NodeInfo<T>
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
            if (other == null) return false;
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

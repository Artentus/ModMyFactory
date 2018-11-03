using System;

namespace ModMyFactory.ModSettings.Serialization
{
    /// <summary>
    /// Represents a version as stored inside Factorio binary files.
    /// </summary>
    class BinaryVersion : IEquatable<BinaryVersion>, IComparable<BinaryVersion>, IComparable
    {
        public static readonly BinaryVersion Empty = new BinaryVersion(0, 0, 0, 0);

        private readonly ulong binaryVersion;

        /// <summary>
        /// The main version (0 for alpha, 1 for release).
        /// </summary>
        public ushort Main => unchecked((ushort)(binaryVersion >> 48));

        /// <summary>
        /// Major version.
        /// </summary>
        public ushort Major => unchecked((ushort)(binaryVersion >> 32));

        /// <summary>
        /// Minor version.
        /// </summary>
        public ushort Minor => unchecked((ushort)(binaryVersion >> 16));

        /// <summary>
        /// Revision. For developer use only.
        /// </summary>
        public ushort Revision => unchecked((ushort)binaryVersion);

        /// <summary>
        /// Create a new binary version object.
        /// </summary>
        /// <param name="main">The main version (0 for alpha, 1 for release).</param>
        /// <param name="major">Major version.</param>
        /// <param name="minor">Minor version.</param>
        /// <param name="revision">Revision. For developer use only.</param>
        public BinaryVersion(ushort main, ushort major, ushort minor, ushort revision)
        {
            binaryVersion = (((ulong)main) << 48) | (((ulong)major) << 32) | (((ulong)minor) << 16) | (ulong)main;
        }
        
        public override string ToString()
        {
            return string.Join(".", Main, Major, Minor, Revision);
        }

        public override int GetHashCode()
        {
            return binaryVersion.GetHashCode();
        }
        
        #region Equals
        public bool Equals(BinaryVersion other)
        {
            if (ReferenceEquals(other, null)) return false;
            return this.binaryVersion == other.binaryVersion;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BinaryVersion;
            return Equals(other);
        }

        public static bool operator ==(BinaryVersion first, BinaryVersion second)
        {
            if (ReferenceEquals(first, null)) return Equals(second, null);
            return first.Equals(second);
        }

        public static bool operator !=(BinaryVersion first, BinaryVersion second)
        {
            if (ReferenceEquals(first, null)) return !Equals(second, null);
            return !first.Equals(second);
        }
        #endregion
        
        #region Compare
        public int CompareTo(BinaryVersion other)
        {
            if (ReferenceEquals(other, null)) return int.MaxValue;
            return this.binaryVersion.CompareTo(other.binaryVersion);
        }

        public int CompareTo(object obj)
        {
            var other = obj as BinaryVersion;
            return CompareTo(other);
        }

        public static bool operator >(BinaryVersion first, BinaryVersion second)
        {
            if (ReferenceEquals(first, null)) return false;
            return first.CompareTo(second) > 0;
        }

        public static bool operator <(BinaryVersion first, BinaryVersion second)
        {
            if (ReferenceEquals(first, null)) return !ReferenceEquals(second, null);
            return first.CompareTo(second) < 0;
        }

        public static bool operator >=(BinaryVersion first, BinaryVersion second)
        {
            if (ReferenceEquals(first, null)) return ReferenceEquals(second, null);
            return first.CompareTo(second) >= 0;
        }

        public static bool operator <=(BinaryVersion first, BinaryVersion second)
        {
            if (ReferenceEquals(first, null)) return true;
            return first.CompareTo(second) <= 0;
        }
        #endregion
    }
}

using System;

namespace ModMyFactory
{
    /// <summary>
    /// A Version object that handles comparisons like Factorio does.
    /// </summary>
    [Serializable]
    sealed class GameCompatibleVersion : IComparable, IComparable<GameCompatibleVersion>, IEquatable<GameCompatibleVersion>
    {
        private readonly Version baseVersion;

        public int Major => Math.Max(baseVersion.Major, 0);
        public int Minor => Math.Max(baseVersion.Minor, 0);
        public int Build => Math.Max(baseVersion.Build, 0);
        public int Revision => Math.Max(baseVersion.Revision, 0);

        public GameCompatibleVersion()
        {
            baseVersion = new Version();
        }

        public GameCompatibleVersion(int major, int minor)
        {
            baseVersion = new Version(major, minor);
        }

        public GameCompatibleVersion(int major, int minor, int build)
        {
            baseVersion = new Version(major, minor, build);
        }

        public GameCompatibleVersion(int major, int minor, int build, int revision)
        {
            baseVersion = new Version(major, minor, build, revision);
        }

        public GameCompatibleVersion(string version)
        {
            baseVersion = new Version(version);
        }

        public GameCompatibleVersion(Version version)
        {
            baseVersion = version;
        }

        public int CompareTo(GameCompatibleVersion other)
        {
            if (ReferenceEquals(other, null)) return 1;

            int result = Major.CompareTo(other.Major);
            if (result == 0) result = Minor.CompareTo(other.Minor);
            if (result == 0) result = Build.CompareTo(other.Build);
            if (result == 0) result = Revision.CompareTo(other.Revision);
            return result;
        }

        public int CompareTo(object obj)
        {
            if (!(ReferenceEquals(obj, null) || (obj is GameCompatibleVersion)))
                throw new ArgumentException("Object must be of type GameCompatibleVersion.", nameof(obj));

            var other = obj as GameCompatibleVersion;
            return CompareTo(other);
        }

        public bool Equals(GameCompatibleVersion other)
        {
            if (ReferenceEquals(other, null)) return false;

            return (this.Major == other.Major)
                && (this.Minor == other.Minor)
                && (this.Build == other.Build)
                && (this.Revision == other.Revision);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GameCompatibleVersion;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Major.GetHashCode() ^ Minor.GetHashCode() ^ Build.GetHashCode() ^ Revision.GetHashCode();
        }

        public override string ToString()
        {
            return baseVersion.ToString();
        }

        public string ToString(int fieldCount)
        {
            return baseVersion.ToString(fieldCount);
        }

        public static GameCompatibleVersion Parse(string input)
        {
            return new GameCompatibleVersion(input);
        }

        public static bool TryParse(string input, out GameCompatibleVersion result)
        {
            bool success = Version.TryParse(input, out var version);

            if (success) result = new GameCompatibleVersion(version);
            else result = null;

            return success;
        }

        public static bool operator ==(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            if (ReferenceEquals(v1, null)) return ReferenceEquals(v2, null);
            else return v1.Equals(v2);
        }

        public static bool operator !=(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            if (ReferenceEquals(v1, null)) return !ReferenceEquals(v2, null);
            else return !v1.Equals(v2);
        }

        public static bool operator <=(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            if (ReferenceEquals(v1, null)) return true;
            else return v1.CompareTo(v2) <= 0;
        }

        public static bool operator >=(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            if (ReferenceEquals(v1, null)) return ReferenceEquals(v2, null);
            else return v1.CompareTo(v2) >= 0;
        }

        public static bool operator <(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            if (ReferenceEquals(v1, null)) return !ReferenceEquals(v2, null);
            else return v1.CompareTo(v2) < 0;
        }

        public static bool operator >(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            if (ReferenceEquals(v1, null)) return false;
            else return v1.CompareTo(v2) > 0;
        }

        public static implicit operator Version(GameCompatibleVersion version)
        {
            return version?.baseVersion;
        }

        public static implicit operator GameCompatibleVersion(Version version)
        {
            if (ReferenceEquals(version, null)) return null;
            else return new GameCompatibleVersion(version);
        }
    }
}

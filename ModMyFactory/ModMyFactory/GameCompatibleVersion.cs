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
        private readonly Version compareVersion;

        public int Major => compareVersion.Major;
        public int Minor => compareVersion.Minor;
        public int Build => compareVersion.Build;
        public int Revision => compareVersion.Revision;
        public short MinorRevision => compareVersion.MinorRevision;
        public short MajorRevision => compareVersion.MajorRevision;

        public GameCompatibleVersion()
        {
            baseVersion = new Version();
            compareVersion = new Version(0, 0, 0, 0);
        }

        public GameCompatibleVersion(int major, int minor)
        {
            baseVersion = new Version(major, minor);
            compareVersion = new Version(major, minor, 0, 0);
        }

        public GameCompatibleVersion(int major, int minor, int build)
        {
            baseVersion = new Version(major, minor, build);
            compareVersion = new Version(major, minor, build, 0);
        }

        public GameCompatibleVersion(int major, int minor, int build, int revision)
        {
            baseVersion = new Version(major, minor, build, revision);
            compareVersion = baseVersion; // Can use same object as all 4 components are populated anyway.
        }

        public GameCompatibleVersion(string version)
        {
            baseVersion = new Version(version);
            compareVersion = new Version(baseVersion.Major, baseVersion.Minor,
                Math.Max(baseVersion.Build, 0), Math.Max(baseVersion.Revision, 0));
        }

        public GameCompatibleVersion(Version version)
        {
            baseVersion = version;
            compareVersion = new Version(baseVersion.Major, baseVersion.Minor,
                Math.Max(baseVersion.Build, 0), Math.Max(baseVersion.Revision, 0));
        }

        public int CompareTo(GameCompatibleVersion other)
        {
            return this.compareVersion.CompareTo(other?.compareVersion);
        }

        public int CompareTo(object obj)
        {
            if ((obj != null) && !(obj is GameCompatibleVersion))
                throw new ArgumentException("Object must be of type GameCompatibleVersion.", nameof(obj));

            var other = obj as GameCompatibleVersion;
            return CompareTo(other);
        }

        public bool Equals(GameCompatibleVersion other)
        {
            return this.compareVersion.Equals(other?.compareVersion);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GameCompatibleVersion;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return compareVersion.GetHashCode();
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
            return v1?.compareVersion == v2?.compareVersion;
        }

        public static bool operator !=(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            return v1?.compareVersion != v2?.compareVersion;
        }

        public static bool operator <=(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            return v1?.compareVersion <= v2?.compareVersion;
        }

        public static bool operator >=(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            return v1?.compareVersion >= v2?.compareVersion;
        }

        public static bool operator <(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            return v1?.compareVersion < v2?.compareVersion;
        }

        public static bool operator >(GameCompatibleVersion v1, GameCompatibleVersion v2)
        {
            return v1?.compareVersion > v2?.compareVersion;
        }

        public static implicit operator Version(GameCompatibleVersion version)
        {
            return version?.baseVersion;
        }

        public static implicit operator GameCompatibleVersion(Version version)
        {
            return new GameCompatibleVersion(version);
        }
    }
}

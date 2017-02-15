using System;
using System.Text;

namespace ModMyFactory
{
    class ExtendedVersion : ICloneable, IComparable, IComparable<ExtendedVersion>, IComparable<Version>, IEquatable<ExtendedVersion>, IEquatable<Version>
    {
        private const string PreReleaseTag = "pre";

        public Version BaseVersion { get; }

        public int PreReleaseVersion { get; }

        public ExtendedVersion()
        {
            BaseVersion = new Version();
            PreReleaseVersion = -1;
        }

        public ExtendedVersion(string version)
        {
            if (version.StartsWith("v.")) version = version.Substring(2);
            else if (version.StartsWith("v")) version = version.Substring(1);

            int index = version.IndexOf(PreReleaseTag, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
            {
                BaseVersion = new Version(version.Substring(0, index));
                PreReleaseVersion = int.Parse(version.Substring(index + PreReleaseTag.Length));
            }
            else
            {
                BaseVersion = new Version(version);
                PreReleaseVersion = -1;
            }
        }

        public ExtendedVersion(Version baseVersion)
        {
            BaseVersion = (Version)baseVersion.Clone();
            PreReleaseVersion = -1;
        }

        public ExtendedVersion(Version baseVersion, int preReleaseVersion)
        {
            if (preReleaseVersion < 0) throw new ArgumentOutOfRangeException(nameof(preReleaseVersion));

            BaseVersion = (Version)baseVersion.Clone();
            PreReleaseVersion = preReleaseVersion;
        }

        public ExtendedVersion(int major, int minor)
        {
            BaseVersion = new Version(major, minor);
            PreReleaseVersion = -1;
        }

        public ExtendedVersion(int major, int minor, int build)
        {
            BaseVersion = new Version(major, minor, build);
            PreReleaseVersion = -1;
        }

        public ExtendedVersion(int major, int minor, int build, int preReleaseVersion)
        {
            if (preReleaseVersion < 0) throw new ArgumentOutOfRangeException(nameof(preReleaseVersion));

            BaseVersion = new Version(major, minor, build);
            PreReleaseVersion = preReleaseVersion;
        }

        public ExtendedVersion(int major, int minor, int build, int preReleaseVersion, int revision)
        {
            if (preReleaseVersion < 0) throw new ArgumentOutOfRangeException(nameof(preReleaseVersion));

            BaseVersion = new Version(major, minor, build, revision);
            PreReleaseVersion = preReleaseVersion;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(BaseVersion);

            if (PreReleaseVersion >= 0)
            {
                sb.Append(PreReleaseTag);
                sb.Append(PreReleaseVersion);
            }

            return sb.ToString();
        }

        public string ToString(int fieldCount)
        {
            if (fieldCount > 5 || fieldCount < 0) throw new ArgumentOutOfRangeException(nameof(fieldCount));

            var sb = new StringBuilder();
            sb.Append(BaseVersion.ToString(Math.Min(fieldCount, 4)));

            if (fieldCount == 5)
            {
                sb.Append(PreReleaseTag);
                sb.Append(Math.Max(PreReleaseVersion, 0));
            }

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return BaseVersion.GetHashCode() ^ PreReleaseVersion.GetHashCode();
        }

        public object Clone()
        {
            if (PreReleaseVersion >= 0)
                return new ExtendedVersion(BaseVersion, PreReleaseVersion);
            else
                return new ExtendedVersion(BaseVersion);
        }

        

        public bool Equals(ExtendedVersion other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return this.BaseVersion == other.BaseVersion && this.PreReleaseVersion == other.PreReleaseVersion;
        }

        public bool Equals(Version other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return this.BaseVersion == other && this.PreReleaseVersion == -1;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;

            var otherExtended = obj as ExtendedVersion;
            if (!object.ReferenceEquals(otherExtended, null)) return Equals(otherExtended);

            var other = obj as Version;
            if (!object.ReferenceEquals(other, null)) return Equals(other);

            return false;
        }

        public int CompareTo(ExtendedVersion other)
        {
            if (object.ReferenceEquals(other, null)) return int.MaxValue;

            int result = this.BaseVersion.CompareTo(other.BaseVersion);
            if (result == 0) return this.PreReleaseVersion.CompareTo(other.PreReleaseVersion);
            return result;
        }

        public int CompareTo(Version other)
        {
            if (object.ReferenceEquals(other, null)) return int.MaxValue;

            int result = this.BaseVersion.CompareTo(other);
            if (result == 0) return this.PreReleaseVersion.CompareTo(-1);
            return result;
        }

        public int CompareTo(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return int.MaxValue;

            var otherExtended = obj as ExtendedVersion;
            if (!object.ReferenceEquals(otherExtended, null)) return CompareTo(otherExtended);

            var other = obj as Version;
            if (!object.ReferenceEquals(other, null)) return CompareTo(other);

            return 0;
        }

        public static bool operator ==(ExtendedVersion left, ExtendedVersion right)
        {
            if (object.ReferenceEquals(left, null)) return object.ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(ExtendedVersion left, ExtendedVersion right)
        {
            if (object.ReferenceEquals(left, null)) return object.ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator <(ExtendedVersion left, ExtendedVersion right)
        {
            return right?.CompareTo(left) > 0;
        }

        public static bool operator >(ExtendedVersion left, ExtendedVersion right)
        {
            return left?.CompareTo(right) > 0;
        }

        public static bool operator <=(ExtendedVersion left, ExtendedVersion right)
        {
            return right?.CompareTo(left) >= 0;
        }

        public static bool operator >=(ExtendedVersion left, ExtendedVersion right)
        {
            return left?.CompareTo(right) >= 0;
        }
    }
}

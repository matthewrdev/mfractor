using System;

namespace MFractor.Versioning
{
    /// <summary>
    /// A semantic version.
    /// </summary>
    public class SemanticVersion
    {
        /// <summary>
        /// The major component of the version.
        /// <para/>
        /// For example: v[major].0.0.0
        /// </summary>
        /// <value>The major component of the version.</value>
        public int Major { get; } = 0;

        /// <summary>
        /// The minor component of the version.
        /// <para/>
        /// For example: v1.[minor].0.0
        /// </summary>
        /// <value>The minor component of the version.</value>
        public int Minor { get; } = 0;

        /// <summary>
        /// The patch component of the version.
        /// <para/>
        /// For example: v1.0.[patch].0
        /// </summary>
        /// <value>The patch component of the version.</value>
        public int Patch { get; } = 0;

        /// <summary>
        /// The revision component of the version.
        /// <para/>
        /// For example: v1.0.0.[revision]
        /// </summary>
        /// <value>The revision component of the version.</value>
        public int Revision { get; } = 0;

        /// <summary>
        /// Initializes a new <see cref="SemanticVersion"/> instance.
        /// </summary>
        public SemanticVersion(int major,
                               int minor)
            : this(major, minor, 0, 0)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SemanticVersion"/> instance.
        /// </summary>
        public SemanticVersion(int major, 
                               int minor, 
                               int patch)
            : this(major, minor, patch, 0)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SemanticVersion"/> instance.
        /// </summary>
        public SemanticVersion(int major, 
                               int minor, 
                               int patch, 
                               int revision)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
        }

        public override string ToString()
        {
            return string.Format($"{Major}.{Minor}.{Patch}.{Revision}");
        }

        /// <summary>
        /// Returns the shortened string representation of this semantic version, excluding the revision number.
        /// <para/>
        /// EG: major.minor.patch
        /// <para/>
        /// EG: 1.0.0
        /// </summary>
        public string ToShortString()
        {
            return string.Format($"{Major}.{Minor}.{Patch}");
        }

        /// <summary>
        /// Generates a string representation of this semantic version, padding out each version component to two digits.
        /// <para/>
        /// For example, the version number 1.2.1 would be represented as 1.02.01 as a padded version string.
        /// </summary>
		public string ToPaddedVersionString(bool includeRevision = false)
		{
            var version = string.Format($"{Major}.{Minor.ToString("00")}.{Patch.ToString("00")}");

            if (includeRevision)
            {
                version += "." + Revision.ToString("00");
            }

            return version;
		}

        /// <summary>
        /// Parse the <paramref name="version"/> into a <see cref="SemanticVersion"/>
        /// </summary>
        public static SemanticVersion Parse(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException($"{nameof(version)} is null or empty.");
            }

            var major = 0;
            var minor = 0;
            var patch = 0;
            var revision = 0;

            var components = version.Split('.');

            if (components.Length >= 1)
            {
                if (!int.TryParse(components[0], out major))
                {
                    throw new InvalidOperationException($"Cannot parse the major segment of the semantic version {version}. {components[0]} is not a valid integer value.");
                }
            }

            if (components.Length >= 2)
            {
                if (!int.TryParse(components[1], out minor))
                {
                    throw new InvalidOperationException($"Cannot parse the minor segment of the semantic version {version}. {components[1]} is not a valid integer value.");
                }
            }

            if (components.Length >= 3)
            {
                if (!int.TryParse(components[2], out patch))
                {
                    throw new InvalidOperationException($"Cannot parse the patch segment of the semantic version {version}. {components[2]} is not a valid integer value.");
                }
            }

            if (components.Length >= 4)
            {
                if (!int.TryParse(components[3], out revision))
                {
                    throw new InvalidOperationException($"Cannot parse the major segment of the semantic version {version}. {components[3]} is not a valid integer value.");
                }
            }

            return new SemanticVersion(major, minor, patch, revision);
        }

        /// <summary>
        /// Tries to parse the <paramref name="version"/> into a <see cref="SemanticVersion"/>.
        /// </summary>
        public static bool TryParse(string version, out SemanticVersion result)
        {
            result = null;

            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            var major = 0;
            var minor = 0;
            var patch = 0;
            var revision = 0;

            var components = version.Split('.');

            if (components.Length >= 1)
            {
                if (!int.TryParse(components[0], out major))
                {
                    return false;
                }
            }

            if (components.Length >= 2)
            {
                if (!int.TryParse(components[1], out minor))
                {
                    return false;
                }
            }

            if (components.Length >= 3)
            {
                if (!int.TryParse(components[2], out patch))
                {
                    return false;
                }
            }

            if (components.Length >= 4)
            {
                if (!int.TryParse(components[3], out revision))
                {
                    return false;
                }
            }

            result = new SemanticVersion(major, minor, patch, revision);
            return true;
        }
    }
}

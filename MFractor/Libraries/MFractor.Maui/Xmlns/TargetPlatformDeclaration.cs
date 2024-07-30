namespace MFractor.Maui.Xmlns
{
    public class TargetPlatformDeclaration
    {
        public string PlatformKeyword { get; }

        public string TargettedPlatform { get; }

        public bool HasTargettedPlatform => !string.IsNullOrEmpty(TargettedPlatform);

        public TargetPlatformDeclaration(string keyword, string name)
        {
            PlatformKeyword = keyword;
            TargettedPlatform = name;
        }
    }

}


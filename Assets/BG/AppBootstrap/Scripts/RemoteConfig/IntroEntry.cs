namespace AppBootstrap.Splash
{
    [System.Serializable]
    internal sealed class IntroEntry
    {
        public IntroLayoutType type;
        public IntroActionType actionType;
        public int introTime;
        public string[] introAdGroupNames;
        public string introText;
        public string introBackgroundName;
        public string introCoverName;

        public bool showBlackBackground;
        public bool showCountDownText;
    }
}

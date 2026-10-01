namespace AppBootstrap.Splash
{
    public readonly struct SplashAdExecutionResult
    {
        public SplashAdExecutionResult(bool primaryReady, bool backupReady, string selectedGroupName,
            string selectedAdsPosition)
        {
            PrimaryReady = primaryReady;
            BackupReady = backupReady;
            SelectedGroupName = selectedGroupName;
            SelectedAdsPosition = selectedAdsPosition;
        }

        public bool PrimaryReady { get; }
        public bool BackupReady { get; }
        public bool IsReady => PrimaryReady || BackupReady;
        public string SelectedGroupName { get; }
        public string SelectedAdsPosition { get; }
    }
}

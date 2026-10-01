namespace AppBootstrap.Splash
{
    internal interface IConfigProvider
    {
        T LoadRemoteConfig<T>(string configKey);
    }
}

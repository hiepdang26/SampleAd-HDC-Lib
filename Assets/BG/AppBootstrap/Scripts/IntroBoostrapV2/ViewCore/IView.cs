namespace AppBootstrap.Intro
{
    public interface IView
    {
        public void RendererView(IntroConfig config);
        public bool CanRendererConfig(IntroConfig config);
        public bool IsCompleteView();
    }
}
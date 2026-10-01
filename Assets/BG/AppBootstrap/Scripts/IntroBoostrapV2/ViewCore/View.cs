using BG_Library.NET.API;
using UnityEngine;

namespace AppBootstrap.Intro
{
    public abstract class View : MonoBehaviour, IView
    {
        public PULayout PuLayout;
        
        public abstract void RendererView(IntroConfig config);
        public abstract bool CanRendererConfig(IntroConfig config);
        public abstract bool IsCompleteView();
    }
}
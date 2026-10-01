using System;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class IntroViewNone : View
    {
        private IntroConfig _config;
        private bool _isCompleteView;
        
        public override void RendererView(IntroConfig config)
        {
            _isCompleteView = true;
            _config = config;
        }

        public override bool CanRendererConfig(IntroConfig config)
        {
            return config.IntroType == IntroType.FORCE_AD;
        }

        public override bool IsCompleteView()
        {
            return _isCompleteView;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Video;

namespace AppBootstrap.Splash
{
    public class VideoLoader : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public GameObject videoRenderer;
        
        private void Awake()
        {
            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.Prepare();
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            videoRenderer.SetActive(true);
            videoPlayer.Play();
        }
    }
}
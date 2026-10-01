using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Splash
{
    public class LoadingViewer : MonoBehaviour
    {
        private static readonly string[] LoadingTexts =
        {
            "Starting the engine...", "Warming up the engine...", "Fueling up the tank...",
            "Checking tire pressure...", "Adjusting the mirrors...", "Buckling up your seatbelt...",
            "Revving the engine...", "Polishing the paint...", "Tuning the radio...",
            "Mapping the city streets...", "Charging the battery...", "Calibrating the GPS...",
            "Painting the road lines...", "Syncing the traffic lights...", "Parking the AI drivers...",
            "Waxing the hood...", "Inflating the tires...", "Loading horsepower...",
            "Greasing the gears...", "Cleaning the windshield...", "Building the city...",
            "Unlocking the garage...", "Shifting into gear...", "Releasing the handbrake...",
            "Testing the brakes...", "Aligning the wheels...", "Heating the seats...",
            "Paving the highway...", "Counting parking spots...", "Tuning the horn...",
            "Setting up traffic...", "Drawing the road map...", "Spawning pedestrians...",
            "Filling the potholes...", "Turning on the headlights...", "Loading the open world...",
            "Pumping the brakes...", "Tightening the bolts...", "Loading your dream car...",
            "Cooling the engine...", "Buffing the rims...", "Rolling out the asphalt...",
            "Topping up the oil...", "Booting the dashboard...", "Connecting the speakers...",
            "Loading nitro boost...", "Opening the city gates...", "Almost ready to drive...",
            "Just a few more miles...", "Hitting the road...",
        };

        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private TMP_Text _processText;
        [SerializeField] private float _timeNextTexting;
        [SerializeField] private Image _fill;

        private float _time;
        private int _currentLoadingTextIndex;

        private void Awake()
        {
            _loadingText.text = LoadingTexts[0];
            _currentLoadingTextIndex = 1;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            if (_time >= _timeNextTexting)
            {
                _time = 0;
                _loadingText.text = LoadingTexts[_currentLoadingTextIndex];
                _currentLoadingTextIndex++;
                if (_currentLoadingTextIndex >= LoadingTexts.Length)
                    _currentLoadingTextIndex = 0;
            }
        }

        public void UpdateProgress(float progress)
        {
            _fill.fillAmount = progress;
            _processText.text = $"{(int)(progress * 100)}%";
        }

        private void SetProgress(float progress, float duration)
        {
            _fill.DOKill();
            _fill.DOFillAmount(progress, duration).OnUpdate(() =>
            {
                _processText.text = $"{(int)(_fill.fillAmount * 100)}%";
            });
        }
    }
}
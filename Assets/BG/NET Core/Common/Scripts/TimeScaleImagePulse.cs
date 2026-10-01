using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.Common
{
    [RequireComponent(typeof(Image))]
    public class TimeScaleImagePulse : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Color colorA = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color colorB = new Color(0.25f, 0.8f, 1f, 1f);
        [SerializeField] private float speed = 1f;

        private void Reset()
        {
            targetImage = GetComponent<Image>();
        }

        private void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();
        }

        private void Update()
        {
            if (targetImage == null)
                return;

            float t = Mathf.PingPong(Time.time * Mathf.Max(0.01f, speed), 1f);
            targetImage.color = Color.Lerp(colorA, colorB, t);
        }
    }
}

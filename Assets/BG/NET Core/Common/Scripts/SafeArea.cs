using UnityEngine;

namespace BG_Library.Common
{
    /// <summary>
    /// Safe area implementation for notched mobile devices. Usage:
    ///  (1) Add this component to the top level of any GUI panel. 
    ///  (2) If the panel uses a full screen background image, then create an immediate child and put the component on that instead, with all other elements childed below it.
    ///      This will allow the background image to stretch to the full extents of the screen behind the notch, which looks nicer.
    ///  (3) For other cases that use a mixture of full horizontal and vertical background stripes, use the Conform X and Y controls on separate elements as needed.
    /// </summary>
    public class SafeArea : MonoBehaviour
    {
        RectTransform Panel;
        Rect LastSafeArea = new Rect(0, 0, 0, 0);

        public bool applyTop = true;
        public bool applyBot = true;
        public bool applyRight = true;
        public bool applyLeft = true;

        void Awake()
        {
            Panel = GetComponent<RectTransform>();

            if (Panel == null)
            {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                Destroy(gameObject);
            }

            Refresh();
        }

        void Refresh()
        {
            Rect safeArea = GetSafeArea();

            if (safeArea != LastSafeArea)
                ApplySafeArea(safeArea);
        }

        Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        void ApplySafeArea(Rect r)
        {
            LastSafeArea = r;

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            var currentAnchorMin = Panel.anchorMin;
            var currentAnchorMax = Panel.anchorMax;

            if (applyTop)
                currentAnchorMax.y = anchorMax.y;
            if (applyRight)
                currentAnchorMax.x = anchorMax.x;
            if (applyBot)
                currentAnchorMin.y = anchorMin.y;
            if (applyLeft)
                currentAnchorMin.x = anchorMin.x;

            Panel.anchorMin = currentAnchorMin;
            Panel.anchorMax = currentAnchorMax;
            
            //Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            //    name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
        }
    }
}

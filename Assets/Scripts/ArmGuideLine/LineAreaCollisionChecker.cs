using UnityEngine;

namespace ArmGuideLine
{
    public class LineAreaCollisionChecker : MonoBehaviour
    {
        [HideInInspector]
        public RectTransform upperLineRect;
        [HideInInspector]
        public RectTransform lowerLineRect;
        [HideInInspector]
        public float lineLength = 150f;
        public float tolerance = 0f;

        /// <summary>
        /// -1=下界外  0=区间内  1=上界外
        /// </summary>
        public BandCollisionState JudgeBandPosition(Vector2 wristCanvasPos)
        {
            if (upperLineRect == null || lowerLineRect == null) return BandCollisionState.Inside;
            float upperY = upperLineRect.anchoredPosition.y;
            float lowerY = lowerLineRect.anchoredPosition.y;
            float yMax = Mathf.Max(upperY, lowerY);
            float yMin = Mathf.Min(upperY, lowerY);

            if (wristCanvasPos.y >= yMax) return BandCollisionState.Above;
            if (wristCanvasPos.y <= yMin) return BandCollisionState.Below;
            return BandCollisionState.Inside;
        }
    }
}
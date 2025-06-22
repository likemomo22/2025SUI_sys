using UnityEngine;

namespace Utils
{
    public static class MathUtils
    {
        public static float Normalize(float value, float min, float max)
        {
            if (Mathf.Approximately(max - min, 0f))
                return 0f;
            return Mathf.Clamp01((value - min) / (max - min));
        }
    }
}
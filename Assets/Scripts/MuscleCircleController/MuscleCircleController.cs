using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace MuscleCircleController
{
    public class MuscleCircleController : MonoBehaviour
    {
        public Image fillImage;
        public float minValue = 0f;
        public float maxValue = 7000f;

        private Color recentColor;
        private Color targetColor;
        private float currentNormalized = 0f;

        private void Start()
        {
            if (fillImage != null)
                recentColor = fillImage.color;
        }

        public void SetValue(float rawValue)
        {
            if (fillImage == null) return;

            // 归一化
            float normalized = MathUtils.Normalize(rawValue, minValue, maxValue);
            currentNormalized = normalized;
            fillImage.fillAmount = normalized;

            // 设置目标颜色
            targetColor = normalized >= 0.4f
                ? Color.red
                : new Color(160f / 255f, 232f / 255f, 180f / 255f);
        }

        private void Update()
        {
            if (fillImage == null) return;

            recentColor = Color.Lerp(recentColor, targetColor, Time.deltaTime * 5f);
            fillImage.color = recentColor;
        }
    }
}
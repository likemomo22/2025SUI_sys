using UnityEngine;
using UnityEngine.UI;
using utils;
using Utils;

namespace MuscleCircleController
{
    public class MuscleCircleController : MonoBehaviour
    {
        public Image fillImage;
        public float minValue;
        public float maxValue = 20000f;
        private float currentNormalized;

        private Color recentColor;
        private Color targetColor;

        private void Start()
        {
            if (fillImage != null)
                recentColor = fillImage.color;
            if (gameObject.name == "TargetMuscleCircleVer3") maxValue = GlobalText.channel1;
            if (gameObject.name == "SubMuscleCircle1Ver3") maxValue = GlobalText.channel2;
            if (gameObject.name == "SubMuscleCircle2Ver3") maxValue = GlobalText.channel3;
        }

        private void Update()
        {
            if (fillImage == null) return;

            recentColor = Color.Lerp(recentColor, targetColor, Time.deltaTime * 5f);
            fillImage.color = recentColor;
        }

        public void SetValue(float rawValue)
        {
            if (fillImage == null) return;

            // 归一化
            var normalized = MathUtils.Normalize(rawValue, minValue, maxValue);
            normalized = Mathf.Clamp01(normalized);
            currentNormalized = normalized;

            fillImage.fillAmount = normalized;

            // 定义颜色
            Color green  = new Color(160f / 255f, 232f / 255f, 180f / 255f);
            Color yellow = new Color(255f / 255f, 227f / 255f, 110f / 255f);
            Color red    = Color.red;

            // === 颜色区间渐变 ===
            if (normalized < 0.2f)
            {
                targetColor = green;
            }
            else if (normalized < 0.3f)
            {
                // 绿色渐变到黄色
                float t = Mathf.InverseLerp(0.2f, 0.3f, normalized);
                targetColor = Color.Lerp(green, yellow, t);
            }
            else if (normalized < 0.4f)
            {
                // 黄色渐变到红色
                float t = Mathf.InverseLerp(0.3f, 0.4f, normalized);
                targetColor = Color.Lerp(yellow, red, t);
            }
            else
            {
                targetColor = red;
            }
        }

    }
}
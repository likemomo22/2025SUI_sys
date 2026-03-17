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

        // ✅ 当前颜色状态：0=绿，1=黄，2=红
        private int currentColorState = 0;

        private void Start()
        {
            if (fillImage != null)
                recentColor = fillImage.color;

            if (gameObject.name == "TargetMuscleCircleVer3") maxValue = GlobalText.channel1*1.5f;
            if (gameObject.name == "SubMuscleCircle1Ver3")   maxValue = GlobalText.channel2*1.5f;
            if (gameObject.name == "SubMuscleCircle2Ver3")   maxValue = GlobalText.channel3*1.5f;
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

            // ---------- 归一化 ----------
            var normalized = MathUtils.Normalize(rawValue, minValue, maxValue);
            normalized = Mathf.Clamp01(normalized);
            currentNormalized = normalized;

            fillImage.fillAmount = normalized;

            // ---------- 颜色定义 ----------
            Color green  = new Color(160f / 255f, 232f / 255f, 180f / 255f);
            Color yellow = new Color(255f / 255f, 227f / 255f, 110f / 255f);
            Color red    = Color.red;

            // ---------- 颜色 & 状态判定 ----------
            if (normalized < 0.2f)
            {
                targetColor = green;
                currentColorState = 0; // 绿色
            }
            else if (normalized < 0.4f)
            {
                float t = Mathf.InverseLerp(0.3f, 0.4f, normalized);
                targetColor = Color.Lerp(yellow, red, t);
                currentColorState = 1; // 黄色
            }
            else
            {
                targetColor = red;
                currentColorState = 2; // 红色
            }
        }

        /// <summary>
        /// ✅ 提供给 CSV / 其他模块使用
        /// </summary>
        public int GetColorState()
        {
            return currentColorState;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using utils;

namespace ArmGuideLine
{
    public class CircleCenterCalculator : MonoBehaviour
    {
        public Button calcButton;              // 拖UI Button进来
        public InputField outputInput;         // 拖InputField进来
        public Canvas targetCanvas;            // 拖Canvas进来
        public float dotRadius = 20f;          // 圆心点半径

        private Image _centerDot;               // 圆心点

        void Start()
        {
            if (calcButton != null)
                calcButton.onClick.AddListener(OnCalcButtonClicked);

            if (outputInput != null)
                outputInput.onEndEdit.AddListener(OnInputChanged);

            if (targetCanvas != null)
            {
                _centerDot = CreateRedDot("CircleCenterDot");
                _centerDot.gameObject.SetActive(false);
            }
        }

        private Image CreateRedDot(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas.transform, false);

            var img = go.GetComponent<Image>();
            img.color = Color.red;
            img.raycastTarget = false;
            img.sprite = UnityEngine.Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Simple;
            img.preserveAspect = true;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(dotRadius, dotRadius);
            return img;
        }

        // 按钮点击，计算圆心并写入InputField
        void OnCalcButtonClicked()
        {
            Vector2 A = GlobalText.CircleTop;
            Vector2 B = GlobalText.CircleMid;
            Vector2 C = GlobalText.CircleBottom;

            Vector2 center;
            bool success = CalcCircleCenter(A, B, C, out center);
            if (success)
            {
                Debug.Log($"圆心为: {center}");
                if (outputInput != null)
                    outputInput.text = $"{center.x:F2},{center.y:F2}";

                if (_centerDot != null)
                {
                    _centerDot.rectTransform.anchoredPosition = center;
                    _centerDot.gameObject.SetActive(true);
                }

                // === 新增：保存到GlobalText ===
                GlobalText.CircleCenter = center;
            }
            else
            {
                Debug.LogWarning("三点共线，无法计算圆心！");
                if (outputInput != null)
                    outputInput.text = "三点共线，无法计算圆心！";
                if (_centerDot != null)
                    _centerDot.gameObject.SetActive(false);
            }
        }

        // 用户手动编辑InputField时
        void OnInputChanged(string input)
        {
            string[] parts = input.Split(',');
            if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
            {
                Vector2 center = new Vector2(x, y);
                if (_centerDot != null)
                {
                    _centerDot.rectTransform.anchoredPosition = center;
                    _centerDot.gameObject.SetActive(true);
                }
                Debug.Log($"通过输入手动设置圆心为: {center}");

                // === 新增：保存到GlobalText ===
                GlobalText.CircleCenter = center;
            }
        }

        // 计算三点外接圆圆心
        public static bool CalcCircleCenter(Vector2 A, Vector2 B, Vector2 C, out Vector2 center)
        {
            float x1 = A.x, y1 = A.y;
            float x2 = B.x, y2 = B.y;
            float x3 = C.x, y3 = C.y;

            float a = x1 - x3;
            float b = y1 - y3;
            float c = x2 - x3;
            float d = y2 - y3;

            float e = a * (x1 + x3) + b * (y1 + y3);
            float f = c * (x2 + x3) + d * (y2 + y3);

            float g = 2.0f * (a * d - b * c);

            if (Mathf.Abs(g) < 1e-6f)
            {
                center = Vector2.zero;
                return false;
            }

            center = new Vector2(
                (d * e - b * f) / g,
                (a * f - c * e) / g
            );
            return true;
        }
    }
}

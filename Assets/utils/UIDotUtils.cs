using UnityEngine;
using UnityEngine.UI;

namespace utils
{
    public static class UIDotUtils
    {
        /// <summary>
        /// 在目标Canvas上创建一个UI圆点
        /// </summary>
        /// <param name="parentCanvas">父Canvas</param>
        /// <param name="color">颜色</param>
        /// <param name="size">像素大小</param>
        /// <returns>创建的RectTransform</returns>
        public static RectTransform CreateUIDot(
            RectTransform parentCanvas,
            Color color,
            float size = 32f)
        {
            GameObject dotObj = new GameObject("WristDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dotObj.transform.SetParent(parentCanvas, false);
            var img = dotObj.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = UnityEngine.Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            var rect = dotObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.gameObject.SetActive(true);
            return rect;
        }
    }
}
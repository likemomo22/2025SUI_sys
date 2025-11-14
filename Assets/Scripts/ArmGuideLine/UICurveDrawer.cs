using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmGuideLine
{
    public class UICurveDrawer : MonoBehaviour
    {
        public RectTransform targetCanvas;
        public float arcLineThickness = 6f;

        private readonly List<RectTransform> innerArcSegments = new();
        private readonly List<RectTransform> outerArcSegments = new();

        // 创建由多段直线组成的双圆弧
        public void CreateDoubleArcUI(
            Vector2 center,
            Vector2 start,
            Vector2 end,
            float baseRadius,
            float innerOffset, // 小于0内缩，>0外扩
            float outerOffset,
            Color color,
            int segmentCount
        )
        {
            // 清空原有
            foreach (var seg in innerArcSegments)
                if (seg)
                    Destroy(seg.gameObject);
            foreach (var seg in outerArcSegments)
                if (seg)
                    Destroy(seg.gameObject);
            innerArcSegments.Clear();
            outerArcSegments.Clear();

            var angleStart = Mathf.Atan2(start.y - center.y, start.x - center.x);
            var angleEnd = Mathf.Atan2(end.y - center.y, end.x - center.x);
            if (angleEnd < angleStart) angleEnd += Mathf.PI * 2f;

            // 内弧点
            var innerPoints = new Vector2[segmentCount + 1];
            var innerRadius = baseRadius + innerOffset;
            for (var i = 0; i <= segmentCount; i++)
            {
                var t = (float)i / segmentCount;
                var angle = Mathf.Lerp(angleStart, angleEnd, t);
                innerPoints[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * innerRadius;
            }

            // 外弧点
            var outerPoints = new Vector2[segmentCount + 1];
            var outerRadius = baseRadius + outerOffset;
            for (var i = 0; i <= segmentCount; i++)
            {
                var t = (float)i / segmentCount;
                var angle = Mathf.Lerp(angleStart, angleEnd, t);
                outerPoints[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * outerRadius;
            }

            // 生成线段
            for (var i = 0; i < segmentCount; i++)
            {
                innerArcSegments.Add(CreateArcLineSegment(innerPoints[i], innerPoints[i + 1], color, arcLineThickness));
                outerArcSegments.Add(CreateArcLineSegment(outerPoints[i], outerPoints[i + 1], color, arcLineThickness));
            }
        }

        // 创建一段UI线
        private RectTransform CreateArcLineSegment(Vector2 p1, Vector2 p2, Color color, float thickness)
        {
            var go = new GameObject("ArcSegment", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = null;
            var rect = go.GetComponent<RectTransform>();
            var dir = p2 - p1;
            var length = dir.magnitude;
            rect.sizeDelta = new Vector2(length, thickness);
            rect.pivot = new Vector2(0, 0.5f); // 左端为锚点
            rect.anchoredPosition = p1;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            return rect;
        }

        // 变色
        public void SetArcsColor(Color c)
        {
            foreach (var seg in innerArcSegments)
                if (seg)
                    seg.GetComponent<Image>().color = c;
            foreach (var seg in outerArcSegments)
                if (seg)
                    seg.GetComponent<Image>().color = c;
        }

        // 单独变色
        public void SetInnerArcColor(Color c)
        {
            foreach (var seg in innerArcSegments)
                if (seg)
                    seg.GetComponent<Image>().color = c;
        }

        public void SetOuterArcColor(Color c)
        {
            foreach (var seg in outerArcSegments)
                if (seg)
                    seg.GetComponent<Image>().color = c;
        }
    }
}
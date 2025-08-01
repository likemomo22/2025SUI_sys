using UnityEngine;

namespace ArmGuideLine
{
    public static class ArcBandMathChecker
    {
        public enum ArcZone
        {
            Inner,      // 比内弧还小
            Outer,      // 比外弧还大
            Between     // 内外弧之间
        }

        /// <summary>
        /// 返回点p属于哪个区间
        /// </summary>
        public static ArcZone GetArcZone(Vector2 p, Vector2 center, float innerR, float outerR, float angleStart, float angleEnd)
        {
            Vector2 rel = p - center;
            float dist = rel.magnitude;
            float minR = Mathf.Min(innerR, outerR);
            float maxR = Mathf.Max(innerR, outerR);

            float angle = Mathf.Atan2(rel.y, rel.x);
            if (angleEnd < angleStart) angleEnd += Mathf.PI * 2f;
            if (angle < angleStart) angle += Mathf.PI * 2f;

            if (angle < angleStart || angle > angleEnd)
                return ArcZone.Outer; // 弧外（你如需区分可自定义）

            if (dist < minR)
                return ArcZone.Inner;
            else if (dist > maxR)
                return ArcZone.Outer;
            else
                return ArcZone.Between;
        }
    }
}
using UnityEngine;

namespace ArmGuideLine
{
    public static class ArcBandMathChecker
    {
        public enum ArcZone
        {
            Inner, // 比内弧还小
            Outer, // 比外弧还大
            Between // 内外弧之间
        }

        /// <summary>
        ///     返回点p属于哪个区间
        /// </summary>
        public static ArcZone GetArcZone(Vector2 p, Vector2 center, float innerR, float outerR, float angleStart,
            float angleEnd)
        {
            var rel = p - center;
            var dist = rel.magnitude;
            var minR = Mathf.Min(innerR, outerR);
            var maxR = Mathf.Max(innerR, outerR);

            var angle = Mathf.Atan2(rel.y, rel.x);
            if (angleEnd < angleStart) angleEnd += Mathf.PI * 2f;
            if (angle < angleStart) angle += Mathf.PI * 2f;

            if (angle < angleStart || angle > angleEnd)
                return ArcZone.Outer; // 弧外（你如需区分可自定义）

            if (dist < minR)
                return ArcZone.Inner;
            if (dist > maxR)
                return ArcZone.Outer;
            return ArcZone.Between;
        }
    }
}
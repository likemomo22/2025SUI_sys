using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using UnityEngine;

namespace utils
{
    public class PoseLandmarkTracker : MonoBehaviour
    {
        public PoseLandmarkerRunner runner;

        /// <summary>
        ///     获取指定 landmark 的 Normalized 坐标（0~1）
        /// </summary>
        public bool TryGetNormalizedLandmarkPosition(int landmarkIndex, out Vector3 position)
        {
            position = Vector3.zero;

            if (runner == null)
                return false;

            var result = runner.CurrentResult;
            if (result.poseLandmarks == null || result.poseLandmarks.Count == 0)
                return false;

            var landmarkList = result.poseLandmarks[0].landmarks;
            if (landmarkList == null || landmarkList.Count <= landmarkIndex)
                return false;

            var lm = landmarkList[landmarkIndex];
            position = new Vector3(lm.x, lm.y, 0);
            return true;
        }

        /// <summary>
        ///     获取指定 landmark 的世界坐标（基于 [-1, 1] 空间映射）
        /// </summary>
        public bool TryGetWorldLandmarkPosition(int landmarkIndex, out Vector3 position)
        {
            position = Vector3.zero;

            if (!TryGetNormalizedLandmarkPosition(landmarkIndex, out var norm))
                return false;

            // 可选：保留转换方式但默认 scale=1
            var x = (norm.x - 0.5f) * 2f;
            var y = -(norm.y - 0.5f) * 2f;
            var z = -norm.z;

            position = new Vector3(x, y, z);
            return true;
        }

        public bool TryGetCanvasLandmarkPosition(int landmarkIndex, RectTransform canvasRect, out Vector2 canvasPos)
        {
            canvasPos = Vector2.zero;

            if (!TryGetNormalizedLandmarkPosition(landmarkIndex, out var norm))
                return false;

            var width = canvasRect.rect.width;
            var height = canvasRect.rect.height;

            // x: [0,1] -> [-width/2, width/2]
            // y: [0,1] -> [-height/2, height/2]，但要注意 y 方向通常是 1-top，0-bottom，所以要 1-norm.y
            var x = (norm.x - 0.5f) * width;
            var y = (0.5f - norm.y) * height; // 上为正，下为负

            canvasPos = new Vector2(x, y);
            return true;
        }
    }
}
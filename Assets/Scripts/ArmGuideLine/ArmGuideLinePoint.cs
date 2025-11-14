using UnityEngine;
using UnityEngine.UI;
using utils;

namespace ArmGuideLine
{
    public class ArmGuideLinePoint : MonoBehaviour
    {
        public PoseLandmarkTracker tracker;
        public Canvas targetCanvas;
        public float dotRadius = 18f;
        private Image bottomDot;
        private Image midDot;

        // 三个圆点实例
        private Image topDot;

        private void Start()
        {
            if (targetCanvas == null) return;

            topDot = CreateRedDot("TopDot");
            midDot = CreateRedDot("MidDot");
            bottomDot = CreateRedDot("BottomDot");
        }

        private void Update()
        {
            if (tracker == null || targetCanvas == null)
                return;

            var canvasRect = targetCanvas.GetComponent<RectTransform>();
            if (canvasRect == null)
                return;

            if (!tracker.TryGetCanvasLandmarkPosition(15, canvasRect, out var currentPos))
                return;

            // 按键监听并赋值
            if (Input.GetKeyDown(KeyCode.T))
            {
                GlobalText.circleTop = currentPos;
                Debug.Log("CircleTop 已记录: " + currentPos);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                GlobalText.circleMid = currentPos;
                Debug.Log("CircleMid 已记录: " + currentPos);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                GlobalText.circleBottom = currentPos;
                Debug.Log("CircleBottom 已记录: " + currentPos);
            }

            // 三个点的位置分别赋值
            topDot.rectTransform.anchoredPosition = GlobalText.circleTop;
            midDot.rectTransform.anchoredPosition = GlobalText.circleMid;
            bottomDot.rectTransform.anchoredPosition = GlobalText.circleBottom;
        }

        // 创建红色圆点
        private Image CreateRedDot(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas.transform, false);

            var img = go.GetComponent<Image>();
            img.color = Color.red;
            img.raycastTarget = false;
            // 使用Unity内置Sprite（圆角rect）
            // img.sprite = UnityEngine.Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Simple;
            img.preserveAspect = true;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(dotRadius, dotRadius);
            return img;
        }
    }
}


// using UnityEngine;
// using utils;
//
// namespace ArmGuideLine
// {
//     public class ArmGuideLinePoint : MonoBehaviour
//     {
//         public PoseLandmarkTracker tracker;
//         public Canvas targetCanvas;
//
//         void Update()
//         {
//             if (tracker == null || targetCanvas == null)
//                 return;
//
//             RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
//             if (canvasRect == null)
//                 return;
//
//             if (!tracker.TryGetCanvasLandmarkPosition(11, canvasRect, out Vector2 currentPos))
//                 return;
//
//             // 按键监听并赋值
//             if (Input.GetKeyDown(KeyCode.T))
//             {
//                 GlobalText.CircleTop = currentPos;
//                 Debug.Log("CircleTop 已记录: " + currentPos);
//             }
//             if (Input.GetKeyDown(KeyCode.C))
//             {
//                 GlobalText.CircleCenter = currentPos;
//                 Debug.Log("CircleCenter 已记录: " + currentPos);
//             }
//             if (Input.GetKeyDown(KeyCode.B))
//             {
//                 GlobalText.CircleBottom = currentPos;
//                 Debug.Log("CircleBottom 已记录: " + currentPos);
//             }
//         }
//     }
// }
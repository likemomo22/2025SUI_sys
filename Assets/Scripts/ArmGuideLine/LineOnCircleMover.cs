using UnityEngine;
using UnityEngine.UI;
using utils;

namespace ArmGuideLine
{
    public class LineOnCircleMover : MonoBehaviour
    {
        public Button moveButton;
        public RectTransform targetCanvas;
        public float lineLength = 150f;
        public float lineThickness = 20f;
        public float lineGap = 80f;
        public float moveDuration = 2.0f;
        public Text counterText;
        public float waitAtBottom = 3f;
        public float waitAtTop = 2f;

        public Text startPointCountdownText;
        public Text endPointCountdownText;

        public LineAreaCollisionChecker checker;
        public PoseLandmarkTracker tracker; // 用于获取landmark

        private RectTransform upperLineRect;
        private RectTransform lowerLineRect;
        private bool moving = false;
        private float moveT = 0f;
        private bool forward = true;

        private Vector2 circleCenter;
        private Vector2 startPoint;
        private Vector2 endPoint;
        private float radius;
        private float startAngle;
        private float endAngle;

        private int roundCounter = 0;
        private float waitTimer = 0f;
        private bool waiting = false;
        private bool waitingForUser = false;
        private bool isAtBottom = true; // 新增


        void Start()
        {
            if (moveButton != null)
                moveButton.onClick.AddListener(OnMoveButtonClicked);

            upperLineRect = CreateLine("UpperLine", Color.blue);
            lowerLineRect = CreateLine("LowerLine", Color.blue);
            upperLineRect.gameObject.SetActive(false);
            lowerLineRect.gameObject.SetActive(false);

            if (counterText != null)
                counterText.text = "0";
            HideAllCountdowns();

            if (checker != null)
            {
                checker.upperLineRect = upperLineRect;
                checker.lowerLineRect = lowerLineRect;
                checker.lineLength = lineLength;
            }
        }

        RectTransform CreateLine(string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = UnityEngine.Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(lineLength, lineThickness);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        void OnMoveButtonClicked()
        {
            circleCenter = GlobalText.CircleCenter;
            startPoint = GlobalText.CircleBottom;
            endPoint = GlobalText.CircleMid;

            radius = Vector2.Distance(startPoint, circleCenter);

            startAngle = Mathf.Atan2(startPoint.y - circleCenter.y, startPoint.x - circleCenter.x);
            endAngle = Mathf.Atan2(endPoint.y - circleCenter.y, endPoint.x - circleCenter.x);
            if (endAngle <= startAngle)
                endAngle += 2 * Mathf.PI;

            moveT = 0f;
            moving = true;
            forward = true;
            roundCounter = 0;
            waitTimer = waitAtBottom;
            waiting = true;
            waitingForUser = false;

            if (counterText != null)
                counterText.text = "0";

            upperLineRect.gameObject.SetActive(true);
            lowerLineRect.gameObject.SetActive(true);

            Vector2 pos = GetPointOnCircle(startAngle);
            SetLinePairPosition(pos);

            HideAllCountdowns();
        }

        void Update()
        {
            if (!moving)
                return;

            if (waiting)
            {
                waitTimer -= Time.deltaTime;
                if (forward)
                {
                    ShowCountdownAt(GetPointOnCircle(startAngle), startPointCountdownText, waitTimer);
                    if (endPointCountdownText != null)
                        endPointCountdownText.gameObject.SetActive(false);
                }
                else
                {
                    ShowCountdownAt(GetPointOnCircle(endAngle), endPointCountdownText, waitTimer);
                    if (startPointCountdownText != null)
                        startPointCountdownText.gameObject.SetActive(false);
                }

                if (waitTimer <= 0f)
                {
                    waiting = false;
                    HideAllCountdowns();
                }
                else
                {
                    return;
                }
            }

            moveT += (forward ? 1 : -1) * (Time.deltaTime / moveDuration);

            if (moveT >= 1f)
            {
                moveT = 1f;
                forward = false;
                waiting = true;
                waitTimer = waitAtTop;
                isAtBottom = false; 
            }
            else if (moveT <= 0f)
            {
                moveT = 0f;
                forward = true;

                roundCounter++;
                if (counterText != null)
                    counterText.text = roundCounter.ToString();

                waiting = true;
                waitTimer = waitAtBottom;
                isAtBottom = true;  
            }

            float theta = Mathf.Lerp(startAngle, endAngle, moveT);
            Vector2 basePos = GetPointOnCircle(theta);
            SetLinePairPosition(basePos);

            // ===== 判定部分 =====
            if (checker != null)
            {
                Vector2 wristCanvasPos = GetWristLandmarkCanvasPosition();
                BandCollisionState state = checker.JudgeBandPosition(wristCanvasPos);

                // 反馈变色（上界红/下界红/区间蓝）
                upperLineRect.GetComponent<Image>().color = (state == BandCollisionState.Above) ? Color.red : Color.blue;
                lowerLineRect.GetComponent<Image>().color = (state == BandCollisionState.Below) ? Color.red : Color.blue;

                // 如要记录到csv：int code = (int)state;
                // _csvLogger.Write(..., code);
            }
        }

        void SetLinePairPosition(Vector2 centerPos)
        {
            Vector2 upPos = centerPos + new Vector2(0, lineGap / 2);
            Vector2 downPos = centerPos - new Vector2(0, lineGap / 2);

            upperLineRect.anchoredPosition = upPos;
            upperLineRect.localRotation = Quaternion.identity;
            lowerLineRect.anchoredPosition = downPos;
            lowerLineRect.localRotation = Quaternion.identity;
        }

        Vector2 GetPointOnCircle(float angle)
        {
            return new Vector2(
                circleCenter.x + Mathf.Cos(angle) * radius,
                circleCenter.y + Mathf.Sin(angle) * radius
            );
        }

        void ShowCountdownAt(Vector2 anchorPos, Text textObj, float seconds)
        {
            if (textObj == null) return;
            textObj.gameObject.SetActive(true);
            textObj.rectTransform.anchoredPosition = anchorPos + new Vector2(0, 30f);
            textObj.text = Mathf.CeilToInt(Mathf.Max(0, seconds)).ToString();
        }
        void HideAllCountdowns()
        {
            if (startPointCountdownText != null) startPointCountdownText.gameObject.SetActive(false);
            if (endPointCountdownText != null) endPointCountdownText.gameObject.SetActive(false);
        }
        
        Vector2 GetWristLandmarkCanvasPosition()
        {
            if (tracker == null || targetCanvas == null)
                return Vector2.zero;

            Vector2 wristCanvasPos;
            int wristLandmarkIndex = 15; // 右手腕（MediaPipe标准）
            bool success = tracker.TryGetCanvasLandmarkPosition(wristLandmarkIndex, targetCanvas, out wristCanvasPos);

            return success ? wristCanvasPos : Vector2.zero;
        }
   
        public MovePhase GetCurrentMovePhase()
        {
            return (waiting && isAtBottom) ? MovePhase.Waiting : MovePhase.Moving;
        }

    }
    public enum MovePhase
    {
        Moving = 1,
        Waiting = 0
    }

}

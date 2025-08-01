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
        public float endLineGap = 100f; // 右竖线与左竖线的水平距离

        public Text bottomCountdownText;
        public Text topCountdownText; // 只用作GO!
        
        public UICurveDrawer arcDrawer; // Inspector拖脚本
        public int desiredSegmentCount = 30; // 平滑度
        public float arcInnerOffset = -25f;
        public float arcOuterOffset = 25f;
        public Color arcNormalColor = Color.green;
        public Color arcHitColor = Color.red;
        
        
        public LineAreaCollisionChecker checker;
        public PoseLandmarkTracker tracker;

        private RectTransform upperLineRect;
        private RectTransform lowerLineRect;
        private Image upperLineImg, lowerLineImg;

        // 两条竖线
        private RectTransform leftEndLineRect;
        private Image leftEndLineImg;
        private RectTransform rightEndLineRect;
        private Image rightEndLineImg;

        private bool moving = false;
        private float moveT = 0f;
        private bool forward = true;

        private Vector2 circleMid;
        private Vector2 endLinePos;  // 竖线的canvas坐标
        private int roundCounter = 0;
        private float bottomTimer = 0f;
        private float topTimer = 0f;
        private bool waitingAtBottom = false;
        private bool waitingAtTop = false;
        private bool isCountingGo = false;
        private bool isTopGoShowed = false; // GO!只显示一次

        private bool hasEnteredBand = false;    // wrist是否已进入band区

        private MovePhase movePhase = MovePhase.Waiting;
            
        public bool showWristDot = true; // Inspector上可勾选是否显示
        private RectTransform wristDotRect;
        private Image wristDotImg;



        void Start()
        {
            if (wristDotRect == null && targetCanvas != null)
            {
                wristDotRect = utils.UIDotUtils.CreateUIDot(targetCanvas, Color.magenta, 32f);
                wristDotImg = wristDotRect.GetComponent<Image>();
                wristDotRect.gameObject.SetActive(showWristDot);
            }
            
            if (moveButton != null)
                moveButton.onClick.AddListener(OnMoveButtonClicked);

            upperLineRect = CreateLine("UpperLine", Color.blue, out upperLineImg);
            lowerLineRect = CreateLine("LowerLine", Color.blue, out lowerLineImg);
            upperLineRect.gameObject.SetActive(false);
            lowerLineRect.gameObject.SetActive(false);

            // 创建左竖线
            leftEndLineRect = CreateVerticalLine("LeftEndLine", Color.gray, out leftEndLineImg);
            leftEndLineRect.gameObject.SetActive(false);
            // 创建右竖线
            rightEndLineRect = CreateVerticalLine("RightEndLine", Color.gray, out rightEndLineImg);
            rightEndLineRect.gameObject.SetActive(false);

            if (counterText != null)
                counterText.text = "0";
            HideAllCountdowns();

            if (checker != null)
            {
                checker.upperLineRect = upperLineRect;
                checker.lowerLineRect = lowerLineRect;
                checker.lineLength = lineLength;
            }

            // 动态生成内外弧，内外偏移距离可调（例：内弧-10，外弧+10像素）
            arcDrawer.CreateDoubleArcUI(
                GlobalText.CircleCenter,   // 圆心
                GlobalText.CircleBottom,   // 起点
                GlobalText.CircleMid,      // 终点
                GlobalText.CircleRadius,   // 基础半径
                arcInnerOffset,                     // 内圆相对半径偏移
                arcOuterOffset,                      // 外圆相对半径偏移
                arcNormalColor,
                desiredSegmentCount        // 段数决定平滑度
            );
        }

        RectTransform CreateLine(string name, Color color, out Image img)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas, false);
            img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = UnityEngine.Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(lineLength, lineThickness);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        RectTransform CreateVerticalLine(string name, Color color, out Image img)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas, false);
            img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = UnityEngine.Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(8f, 120f); // 竖线宽度8，高度120像素
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        void OnMoveButtonClicked()
        {
            circleMid = GlobalText.CircleMid;

            moveT = 0f;
            moving = true;
            forward = true;
            roundCounter = 0;
            bottomTimer = waitAtBottom;
            topTimer = waitAtTop;
            waitingAtBottom = true;
            waitingAtTop = false;
            isCountingGo = false;
            isTopGoShowed = false;

            movePhase = MovePhase.Waiting;
            hasEnteredBand = false;

            if (counterText != null)
                counterText.text = "0";

            upperLineRect.gameObject.SetActive(true);
            lowerLineRect.gameObject.SetActive(true);

            SetLinePairPosition(circleMid);

            // 竖线位置（以 GlobalText.CircleBottom 为左线，右线右移 endLineGap）
            endLinePos = GlobalText.CircleBottom;
            leftEndLineRect.anchoredPosition = new Vector2(endLinePos.x, endLinePos.y);
            rightEndLineRect.anchoredPosition = new Vector2(endLinePos.x + endLineGap, endLinePos.y);
            leftEndLineRect.gameObject.SetActive(true);
            rightEndLineRect.gameObject.SetActive(true);

            HideAllCountdowns();
            if (bottomCountdownText != null)
                bottomCountdownText.gameObject.SetActive(true);

            // 动态生成内外弧，内外偏移距离可调（例：内弧-10，外弧+10像素）
            arcDrawer.CreateDoubleArcUI(
                GlobalText.CircleCenter,   // 圆心
                GlobalText.CircleBottom,   // 起点
                GlobalText.CircleMid,      // 终点
                GlobalText.CircleRadius,   // 基础半径
                -20f,                     // 内圆相对半径偏移
                20f,                      // 外圆相对半径偏移
                arcNormalColor,
                desiredSegmentCount        // 段数决定平滑度
            );
        }

        void Update()
        {
            Vector2 wristCanvasPos = GetWristLandmarkCanvasPosition();

            if (wristDotRect != null)
            {
                wristDotRect.gameObject.SetActive(showWristDot); // 实时开关
                wristDotRect.anchoredPosition = wristCanvasPos;
            }
            
            if (!moving)
                return;

            // 底部倒计时阶段
            if (waitingAtBottom)
            {
                if (bottomTimer > 0f)
                {
                    ShowBottomCountdown(bottomTimer);
                    movePhase = MovePhase.Waiting;
                    bottomTimer -= Time.deltaTime;
                }
                else if (!isCountingGo)
                {
                    ShowBottomGo();
                    isCountingGo = true;
                    movePhase = MovePhase.Waiting;
                    Invoke(nameof(StartMoveUp), 0.7f);
                }
                return;
            }

            // 顶部倒计时阶段
            if (waitingAtTop)
            {
                float tNorm = Mathf.Clamp01(1f - topTimer / waitAtTop);
                Color lerpColor = Color.Lerp(Color.red, Color.green, tNorm);
                upperLineImg.color = lerpColor;
                lowerLineImg.color = lerpColor;

                if (topTimer <= 0f && !isTopGoShowed)
                {
                    ShowTopGo();
                    isTopGoShowed = true;
                    Invoke(nameof(EndTopCountdown), 0.7f);
                    return;
                }
                if (topTimer > 0f)
                {
                    HideAllCountdowns();
                }
                movePhase = MovePhase.TopWaiting;
                topTimer -= Time.deltaTime;
                return;
            }

            // 运动阶段
            float speed = Time.deltaTime / moveDuration;
            if (forward)
            {
                if (moveT < 1f)
                {
                    moveT += speed;
                    if (moveT > 1f) moveT = 1f;
                }
                movePhase = MovePhase.MovingUp;
            }
            else
            {
                moveT -= speed;
                if (moveT < 0f) moveT = 0f;
                movePhase = MovePhase.MovingDown;
            }

            SetLinePairPosition(circleMid);

            // -----------【弧区碰撞判定与变色】-----------
            Vector2 arcCenter = GlobalText.CircleCenter;
            Vector2 arcStart = GlobalText.CircleBottom;
            Vector2 arcEnd   = GlobalText.CircleMid;
            float arcBaseRadius = GlobalText.CircleRadius;

// 获取判定点
            float innerR = arcBaseRadius + arcInnerOffset;
            float outerR = arcBaseRadius + arcOuterOffset;
            float angleStart = Mathf.Atan2(arcStart.y - arcCenter.y, arcStart.x - arcCenter.x);
            float angleEnd   = Mathf.Atan2(arcEnd.y - arcCenter.y, arcEnd.x - arcCenter.x);

            var zone = ArcBandMathChecker.GetArcZone(
                wristCanvasPos, arcCenter, innerR, outerR, angleStart, angleEnd
            );
            if (zone == ArcBandMathChecker.ArcZone.Inner)
            {
                arcDrawer.SetInnerArcColor(arcHitColor);
                arcDrawer.SetOuterArcColor(arcNormalColor);
            }
            else if (zone == ArcBandMathChecker.ArcZone.Outer)
            {
                arcDrawer.SetInnerArcColor(arcNormalColor);
                arcDrawer.SetOuterArcColor(arcHitColor);
            }
            else // Between
            {
                arcDrawer.SetInnerArcColor(arcNormalColor);
                arcDrawer.SetOuterArcColor(arcNormalColor);
            }
            
            // 判定部分
            if (checker != null)
            {
                BandCollisionState state = checker.JudgeBandPosition(wristCanvasPos);

                // 进入band区间时触发顶部倒计时，线全红
                if (forward && !hasEnteredBand && state == BandCollisionState.Inside)
                {
                    waitingAtTop = true;
                    topTimer = waitAtTop;
                    upperLineImg.color = Color.red;
                    lowerLineImg.color = Color.red;
                    HideAllCountdowns();
                    movePhase = MovePhase.TopWaiting;
                    hasEnteredBand = true;
                    return;
                }
                if (!waitingAtTop)
                {
                    upperLineImg.color = (state == BandCollisionState.Above) ? Color.red : Color.blue;
                    lowerLineImg.color = (state == BandCollisionState.Below) ? Color.red : Color.blue;
                }

                // 两条竖线“回到左侧”判定（只要 x < 左线x 就算回到左侧）
                float wristX = wristCanvasPos.x;
                float leftLineX = leftEndLineRect.anchoredPosition.x;
                float rightLineX = rightEndLineRect.anchoredPosition.x;

                if (!waitingAtBottom && !waitingAtTop && !forward)
                {
                    // 只要手腕完全回到两条竖线左侧就判定为动作结束
                    if (wristX < leftLineX)
                    {
                        ToBottomWaitingState();
                        return;
                    }
                }
            }

            // 到顶端但 wrist 未进区间，停在顶端等
            if (moveT >= 1f && forward && !hasEnteredBand)
            {
                moveT = 1f;
                movePhase = MovePhase.MovingUp;
                return;
            }
            // 到底部（兜底）
            else if (moveT <= 0f && !forward)
            {
                // 不需要做动作，实际结束靠左侧判定
            }
        }

        void ToBottomWaitingState()
        {
            roundCounter++;
            if (counterText != null)
                counterText.text = roundCounter.ToString();
            waitingAtBottom = true;
            isCountingGo = false;
            bottomTimer = waitAtBottom;
            ShowBottomCountdown(bottomTimer);
            movePhase = MovePhase.Waiting;
            forward = true;
            hasEnteredBand = false;
            isTopGoShowed = false;
        }

        void StartMoveUp()
        {
            HideAllCountdowns();
            waitingAtBottom = false;
            moveT = 0f;
            forward = true;
        }

        void EndTopCountdown()
        {
            HideAllCountdowns();
            waitingAtTop = false;
            forward = false;
            moveT = 1f;
            hasEnteredBand = false;
            isTopGoShowed = false;
            upperLineImg.color = Color.green;
            lowerLineImg.color = Color.green;
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

        void ShowBottomCountdown(float seconds)
        {
            if (bottomCountdownText == null) return;
            bottomCountdownText.gameObject.SetActive(true);
            if (seconds > 0.5f)
                bottomCountdownText.text = Mathf.CeilToInt(seconds).ToString();
            else
                bottomCountdownText.text = "1";
        }
        void ShowBottomGo()
        {
            if (bottomCountdownText == null) return;
            bottomCountdownText.text = "GO!";
            bottomCountdownText.gameObject.SetActive(true);
        }
        void ShowTopGo()
        {
            if (topCountdownText == null) return;
            topCountdownText.text = "GO!";
            topCountdownText.gameObject.SetActive(true);
        }
        void HideAllCountdowns()
        {
            if (bottomCountdownText != null) bottomCountdownText.gameObject.SetActive(false);
            if (topCountdownText != null) topCountdownText.gameObject.SetActive(false);
        }

        Vector2 GetWristLandmarkCanvasPosition()
        {
            if (tracker == null || targetCanvas == null)
                return Vector2.zero;

            Vector2 wristCanvasPos;
            int wristLandmarkIndex = 15; // 右手腕
            bool success = tracker.TryGetCanvasLandmarkPosition(wristLandmarkIndex, targetCanvas, out wristCanvasPos);

            return success ? wristCanvasPos : Vector2.zero;
        }

        public MovePhase GetCurrentMovePhase()
        {
            return movePhase;
        }
        
    }

    public enum MovePhase
    {
        Waiting = 0,
        MovingUp = 1,
        MovingDown = -1,
        TopWaiting = 2
    }
}

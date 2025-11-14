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

        public bool showWristDot = true; // Inspector上可勾选是否显示
        private float bottomTimer;

        private Vector2 circleMid;
        private Vector2 endLinePos; // 竖线的canvas坐标
        private bool forward = true;

        private bool hasEnteredBand; // wrist是否已进入band区
        private bool isCountingGo;
        private bool isTopGoShowed; // GO!只显示一次
        private Image leftEndLineImg;

        // 两条竖线
        private RectTransform leftEndLineRect;
        private RectTransform lowerLineRect;

        private MovePhase movePhase = MovePhase.Waiting;
        private float moveT;

        private bool moving;
        private Image rightEndLineImg;
        private RectTransform rightEndLineRect;
        private int roundCounter;
        private float topTimer;
        private Image upperLineImg, lowerLineImg;

        private RectTransform upperLineRect;
        private bool waitingAtBottom;
        private bool waitingAtTop;
        private Image wristDotImg;
        private RectTransform wristDotRect;


        private void Start()
        {
            if (wristDotRect == null && targetCanvas != null)
            {
                wristDotRect = UIDotUtils.CreateUIDot(targetCanvas, Color.magenta);
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
                GlobalText.circleCenter, // 圆心
                GlobalText.circleBottom, // 起点
                GlobalText.circleMid, // 终点
                GlobalText.circleRadius, // 基础半径
                arcInnerOffset, // 内圆相对半径偏移
                arcOuterOffset, // 外圆相对半径偏移
                arcNormalColor,
                desiredSegmentCount // 段数决定平滑度
            );
        }

        private void Update()
        {
            var wristCanvasPos = GetWristLandmarkCanvasPosition();

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
                if (topTimer > 0f)
                {
                    ShowTopCountdown(topTimer); // <-- 新增方法
                    movePhase = MovePhase.TopWaiting;
                    topTimer -= Time.deltaTime;
                }
                else if (!isTopGoShowed)
                {
                    ShowTopGo();
                    isTopGoShowed = true;
                    Invoke(nameof(EndTopCountdown), 0.7f);
                }

                return;
            }

            // 运动阶段
            var speed = Time.deltaTime / moveDuration;
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
            var arcCenter = GlobalText.circleCenter;
            var arcStart = GlobalText.circleBottom;
            var arcEnd = GlobalText.circleMid;
            var arcBaseRadius = GlobalText.circleRadius;

// 获取判定点
            var innerR = arcBaseRadius + arcInnerOffset;
            var outerR = arcBaseRadius + arcOuterOffset;
            var angleStart = Mathf.Atan2(arcStart.y - arcCenter.y, arcStart.x - arcCenter.x);
            var angleEnd = Mathf.Atan2(arcEnd.y - arcCenter.y, arcEnd.x - arcCenter.x);

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
                var state = checker.JudgeBandPosition(wristCanvasPos);

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
                    upperLineImg.color = state == BandCollisionState.Above ? Color.red : Color.blue;
                    lowerLineImg.color = state == BandCollisionState.Below ? Color.red : Color.blue;
                }

                // 两条竖线“回到左侧”判定（只要 x < 左线x 就算回到左侧）
                var wristX = wristCanvasPos.x;
                var leftLineX = leftEndLineRect.anchoredPosition.x;
                var rightLineX = rightEndLineRect.anchoredPosition.x;

                if (!waitingAtBottom && !waitingAtTop && !forward)
                    // 只要手腕完全回到两条竖线左侧就判定为动作结束
                    if (wristX < leftLineX)
                    {
                        ToBottomWaitingState();
                        return;
                    }
            }

            // 到顶端但 wrist 未进区间，停在顶端等
            if (moveT >= 1f && forward && !hasEnteredBand)
            {
                moveT = 1f;
                movePhase = MovePhase.MovingUp;
            }
            // 到底部（兜底）
            else if (moveT <= 0f && !forward)
            {
                // 不需要做动作，实际结束靠左侧判定
            }
        }

        private RectTransform CreateLine(string name, Color color, out Image img)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas, false);
            img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(lineLength, lineThickness);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private RectTransform CreateVerticalLine(string name, Color color, out Image img)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(targetCanvas, false);
            img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(8f, 120f); // 竖线宽度8，高度120像素
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private void OnMoveButtonClicked()
        {
            circleMid = GlobalText.circleMid;

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
            endLinePos = GlobalText.circleBottom;
            leftEndLineRect.anchoredPosition = new Vector2(endLinePos.x, endLinePos.y);
            rightEndLineRect.anchoredPosition = new Vector2(endLinePos.x + endLineGap, endLinePos.y);
            leftEndLineRect.gameObject.SetActive(true);
            rightEndLineRect.gameObject.SetActive(true);

            HideAllCountdowns();
            if (bottomCountdownText != null)
                bottomCountdownText.gameObject.SetActive(true);

            // 动态生成内外弧，内外偏移距离可调（例：内弧-10，外弧+10像素）
            arcDrawer.CreateDoubleArcUI(
                GlobalText.circleCenter, // 圆心
                GlobalText.circleBottom, // 起点
                GlobalText.circleMid, // 终点
                GlobalText.circleRadius, // 基础半径
                -20f, // 内圆相对半径偏移
                20f, // 外圆相对半径偏移
                arcNormalColor,
                desiredSegmentCount // 段数决定平滑度
            );
        }

        private void ShowTopCountdown(float seconds)
        {
            if (topCountdownText == null) return;
            topCountdownText.gameObject.SetActive(true);
            if (seconds > 0.5f)
                topCountdownText.text = Mathf.CeilToInt(seconds).ToString();
            else
                topCountdownText.text = "1";
        }

        private void ToBottomWaitingState()
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

        private void StartMoveUp()
        {
            HideAllCountdowns();
            waitingAtBottom = false;
            moveT = 0f;
            forward = true;
        }

        private void EndTopCountdown()
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

        private void SetLinePairPosition(Vector2 centerPos)
        {
            var upPos = centerPos + new Vector2(0, lineGap / 2);
            var downPos = centerPos - new Vector2(0, lineGap / 2);

            upperLineRect.anchoredPosition = upPos;
            upperLineRect.localRotation = Quaternion.identity;
            lowerLineRect.anchoredPosition = downPos;
            lowerLineRect.localRotation = Quaternion.identity;
        }

        private void ShowBottomCountdown(float seconds)
        {
            if (bottomCountdownText == null) return;
            bottomCountdownText.gameObject.SetActive(true);
            if (seconds > 0.5f)
                bottomCountdownText.text = Mathf.CeilToInt(seconds).ToString();
            else
                bottomCountdownText.text = "1";
        }

        private void ShowBottomGo()
        {
            if (bottomCountdownText == null) return;
            bottomCountdownText.text = "GO!";
            bottomCountdownText.gameObject.SetActive(true);
        }

        private void ShowTopGo()
        {
            if (topCountdownText == null) return;
            topCountdownText.text = "GO!";
            topCountdownText.gameObject.SetActive(true);
        }

        private void HideAllCountdowns()
        {
            if (bottomCountdownText != null) bottomCountdownText.gameObject.SetActive(false);
            if (topCountdownText != null) topCountdownText.gameObject.SetActive(false);
        }

        private Vector2 GetWristLandmarkCanvasPosition()
        {
            if (tracker == null || targetCanvas == null)
                return Vector2.zero;

            Vector2 wristCanvasPos;
            var wristLandmarkIndex = 15; // 右手腕
            var success = tracker.TryGetCanvasLandmarkPosition(wristLandmarkIndex, targetCanvas, out wristCanvasPos);

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
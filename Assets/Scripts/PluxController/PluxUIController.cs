using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using utils;
using Utils;
using ArmGuideLine;

namespace PluxController
{
    public class PluxUIController : MonoBehaviour
    {
        private const int MaxScanRetry = 2;
        private const int MaxConnectRetry = 2;

        [Header("UI")]
        public GameObject popupPanel;
        public Button openPopupButton;
        public Button closePopupButton;

        public Button scanButton;
        public Button connectButton;
        public Button startButton;
        public Button stopButton;
        public Button disconnectButton;

        [Header("Acquisition")]
        public int SamplingRate = 1000;
        
        //----------AH--------------
        // public int SamplingRate = 600;
        
        [Header("Judgement")]
        public LineAreaCollisionChecker checker;
        public PoseLandmarkTracker tracker;
        public RectTransform targetCanvas;
        public LineOnCircleMover mover;

        [Header("Muscle Circles")]
        public MuscleCircleController.MuscleCircleController[] muscleCircleControllers;

        private readonly CsvLogger _csvLogger = new();
        private PluxDataProcessor _dataProcessor;
        private PluxDeviceManager _pluxManager;

        private bool _isScanning;
        private bool _isConnecting;
        private bool _isConnected;
        private bool _isAcquisitionRunning;

        private int _scanRetryCount;
        private int _connectRetryCount;
        private string _selectedMac = "";

        #region Unity Lifecycle

        [Obsolete("Obsolete")]
        private void Start()
        {
            openPopupButton.onClick.AddListener(() => popupPanel.SetActive(true));
            closePopupButton.onClick.AddListener(() => popupPanel.SetActive(false));

            scanButton.onClick.AddListener(OnScanClick);
            connectButton.onClick.AddListener(OnConnectClick);
            startButton.onClick.AddListener(OnStartClick);
            stopButton.onClick.AddListener(OnStopClick);
            disconnectButton.onClick.AddListener(OnDisconnectClick);

            _dataProcessor = new PluxDataProcessor(muscleCircleControllers.Length);
            _dataProcessor.OnSmoothedValueChanged += UpdateUIWithSmoothedValue;

            _pluxManager = new PluxDeviceManager(
                ScanResults,
                ConnectionDone,
                AcquisitionStarted,
                OnDataReceived,
                OnEventDetected,
                OnExceptionRaised
            );

            popupPanel.SetActive(false);
            UpdateButtonState();
        }

        #endregion

        #region UI Control

        private void UpdateButtonState()
        {
            bool busy = _isScanning || _isConnecting;

            scanButton.interactable       = !busy;
            connectButton.interactable    = !busy && !string.IsNullOrEmpty(_selectedMac);
            startButton.interactable      = !busy && _isConnected && !_isAcquisitionRunning;
            stopButton.interactable       = _isAcquisitionRunning;
            disconnectButton.interactable = _isConnected && !_isAcquisitionRunning;
        }

        #endregion

        #region Scan & Connect

        [Obsolete("Obsolete")]
        private void OnScanClick()
        {
            if (_isScanning) return;

            Debug.Log("🔍 开始扫描设备");
            _isScanning = true;
            _scanRetryCount = 0;
            UpdateButtonState();

            _pluxManager.GetDetectableDevicesUnity(new List<string> { "BTH" });
        }

        private void OnConnectClick()
        {
            if (_isConnecting || string.IsNullOrEmpty(_selectedMac)) return;

            Debug.Log($"🔗 尝试连接 {_selectedMac}");
            _isConnecting = true;
            _connectRetryCount = 0;
            UpdateButtonState();

            _pluxManager.PluxDev(_selectedMac);
        }

        [Obsolete("Obsolete")]
        private void ScanResults(List<string> listDevices)
        {
            if (listDevices.Count > 0)
            {
                _selectedMac = listDevices[0];
                Debug.Log("✅ 发现设备: " + _selectedMac);

                _isScanning = false;
                UpdateButtonState();

                // 自动连接
                OnConnectClick();
            }
            else
            {
                _scanRetryCount++;
                Debug.LogWarning($"❌ 扫描失败 {_scanRetryCount}/{MaxScanRetry}");

                if (_scanRetryCount < MaxScanRetry)
                {
                    _pluxManager.GetDetectableDevicesUnity(new List<string> { "BTH" });
                }
                else
                {
                    _isScanning = false;
                    UpdateButtonState();
                    Debug.LogError("🚫 多次扫描失败");
                }
            }
        }

        private void ConnectionDone(bool status)
        {
            if (status)
            {
                Debug.Log("✅ 设备连接成功");
                _isConnected = true;
                _isConnecting = false;
            }
            else
            {
                _connectRetryCount++;
                Debug.LogWarning($"❌ 连接失败 {_connectRetryCount}/{MaxConnectRetry}");

                if (_connectRetryCount < MaxConnectRetry)
                {
                    _pluxManager.PluxDev(_selectedMac);
                    return;
                }

                _isConnecting = false;
                _isConnected = false;
            }

            UpdateButtonState();
        }

        #endregion

        #region Acquisition

        private void OnStartClick()
        {
            if (!_isConnected || _isAcquisitionRunning) return;

            Debug.Log("▶️ 开始采集");
            _csvLogger.Init();

            _pluxManager.StartAcquisitionUnity(
                SamplingRate,
                new List<int> { 1, 2, 3 },
                16
            );
        }

        private void AcquisitionStarted(bool success, bool exceptionRaised, string msg)
        {
            _isAcquisitionRunning = success;
            Debug.Log($"🎬 采集状态: {(success ? "成功" : "失败")} {msg}");
            UpdateButtonState();
        }

        private void OnStopClick()
        {
            if (!_isAcquisitionRunning) return;

            _pluxManager.StopAcquisitionUnity();
            _isAcquisitionRunning = false;
            _csvLogger.FinalizeLog();
            UpdateButtonState();

            Debug.Log("⏹ 采集停止");
        }

        private void OnDisconnectClick()
        {
            if (!_isConnected) return;

            _pluxManager.DisconnectPluxDev();
            _isConnected = false;
            _isAcquisitionRunning = false;
            UpdateButtonState();

            Debug.Log("❌ 设备断开");
        }

        #endregion

        #region Data Callback

        private void OnDataReceived(int nSeq, int[] data)
        {
            // ---------- 位姿判定 ----------
            var wristCanvasPos = GetWristLandmarkCanvasPosition();

            var arcCenter = GlobalText.circleCenter;
            var arcStart  = GlobalText.circleBottom;
            var arcEnd    = GlobalText.circleMid;
            var radius    = GlobalText.circleRadius;

            var innerR = radius + mover.arcInnerOffset;
            var outerR = radius + mover.arcOuterOffset;

            var angleStart = Mathf.Atan2(arcStart.y - arcCenter.y, arcStart.x - arcCenter.x);
            var angleEnd   = Mathf.Atan2(arcEnd.y   - arcCenter.y, arcEnd.x   - arcCenter.x);

            var arcZone = ArcBandMathChecker.GetArcZone(
                wristCanvasPos, arcCenter, innerR, outerR, angleStart, angleEnd
            );

            int arcStateForCsv = arcZone switch
            {
                ArcBandMathChecker.ArcZone.Inner   => -1,
                ArcBandMathChecker.ArcZone.Outer   => 1,
                _                                  => 0
            };

            var judgeState = checker != null
                ? (int)checker.JudgeBandPosition(wristCanvasPos)
                : 0;

            int movePhase = (int)mover.GetCurrentMovePhase();

            // ✅ 颜色状态（目标肌）
            int targetColorState = muscleCircleControllers[0].GetColorState();
            int subColorState    = muscleCircleControllers[1].GetColorState();


            // ---------- CSV ----------
            _csvLogger.Write(
                nSeq,
                data,
                arcStateForCsv,
                judgeState,
                movePhase,
                targetColorState,
                subColorState
            );


            _dataProcessor.Process(data);
        }

        private Vector2 GetWristLandmarkCanvasPosition()
        {
            if (tracker == null || targetCanvas == null) return Vector2.zero;

            const int wristIndex = 15;
            return tracker.TryGetCanvasLandmarkPosition(
                wristIndex,
                targetCanvas,
                out var pos
            ) ? pos : Vector2.zero;
        }

        #endregion

        #region Misc

        private void OnEventDetected(PluxDeviceManager.PluxEvent e)
        {
            Debug.Log("🧭 Event: " + e.type);
        }

        private void OnExceptionRaised(int code, string desc)
        {
            Debug.LogError($"⚠️ Exception [{code}]: {desc}");
        }

        private void UpdateUIWithSmoothedValue(int channelIndex, float value)
        {
            if (channelIndex < muscleCircleControllers.Length)
                muscleCircleControllers[channelIndex].SetValue(value);
        }

        #endregion
    }
}

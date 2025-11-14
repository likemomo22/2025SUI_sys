using System;
using System.Collections.Generic;
using ArmGuideLine;
using UnityEngine;
using UnityEngine.UI;
using utils;
using Utils;

namespace PluxController
{
    public class PluxUIController : MonoBehaviour
    {
        [Header("UI Elements")] public GameObject popupPanel;

        public Button openPopupButton;
        public Button closePopupButton;

        public Button scanButton;
        public Button connectButton;
        public Button startButton;
        public Button stopButton;
        public Button disconnectButton;

        public int SamplingRate = 1000;

        public LineAreaCollisionChecker checker;
        public PoseLandmarkTracker tracker;
        public RectTransform targetCanvas; // 用于canvas坐标转换

        public LineOnCircleMover mover;

        // ✅ 多个 UI 圆圈控制器（在 Inspector 中设置）
        public MuscleCircleController.MuscleCircleController[] muscleCircleControllers;

        private readonly CsvLogger _csvLogger = new();

        // ✅ 多通道数据处理器（初始化时设置通道数）
        private PluxDataProcessor _dataProcessor;
        private bool _isAcquisitionRunning;

        private bool _isConnected;

        private PluxDeviceManager _pluxManager;
        private string _selectedMac = "";

        [Obsolete("Obsolete")]
        private void Start()
        {
            openPopupButton.onClick.AddListener(OpenPopup);
            closePopupButton.onClick.AddListener(ClosePopup);
            scanButton.onClick.AddListener(OnScanClick);
            connectButton.onClick.AddListener(OnConnectClick);
            startButton.onClick.AddListener(OnStartClick);
            stopButton.onClick.AddListener(OnStopClick);
            disconnectButton.onClick.AddListener(OnDisconnectClick);

            // ✅ 初始化多通道处理器（根据 UI 控制器数量）
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
        }

        private void OpenPopup()
        {
            popupPanel.SetActive(true);
        }

        private void ClosePopup()
        {
            popupPanel.SetActive(false);
        }

        [Obsolete("Obsolete")]
        private void OnScanClick()
        {
            Debug.Log("开始扫描设备...");
            _pluxManager.GetDetectableDevicesUnity(new List<string> { "BTH" });
        }

        private void OnConnectClick()
        {
            if (!string.IsNullOrEmpty(_selectedMac))
            {
                Debug.Log("尝试连接设备: " + _selectedMac);
                _pluxManager.PluxDev(_selectedMac);
            }
            else
            {
                Debug.LogWarning("请先扫描设备！");
            }
        }

        private void OnStartClick()
        {
            if (!_isConnected)
            {
                Debug.LogError("❌ 未连接设备，不能开始采集！");
                return;
            }

            if (_isAcquisitionRunning)
            {
                Debug.LogWarning("⚠️ 已在采集中，无需重复开始。");
                return;
            }

            Debug.Log("▶️ 开始采集数据...");
            try
            {
                _csvLogger.Init();
                _pluxManager.StartAcquisitionUnity(SamplingRate, new List<int> { 1, 2, 3 }, 16); // 根据需要设置通道编号
            }
            catch (Exception ex)
            {
                Debug.LogError("采集启动失败: " + ex.Message);
            }
        }

        private void OnStopClick()
        {
            if (!_isAcquisitionRunning)
            {
                Debug.LogWarning("⚠️ 当前没有正在进行的采集。");
                return;
            }

            var result = _pluxManager.StopAcquisitionUnity();
            Debug.Log("⏹ 采集已停止（是否强制）: " + result);
            _isAcquisitionRunning = false;
            _csvLogger.FinalizeLog();
        }

        private void OnDisconnectClick()
        {
            if (!_isConnected)
            {
                Debug.LogWarning("⚠️ 尚未连接设备，无需断开。");
                return;
            }

            _pluxManager.DisconnectPluxDev();
            Debug.Log("❌ 设备已断开");
            _isConnected = false;
            _isAcquisitionRunning = false;
        }

        private void ScanResults(List<string> listDevices)
        {
            if (listDevices.Count > 0)
            {
                _selectedMac = listDevices[0];
                Debug.Log("✅ 发现设备: " + _selectedMac);
            }
            else
            {
                Debug.LogWarning("❌ 未找到任何设备");
            }
        }

        private void ConnectionDone(bool status)
        {
            _isConnected = status;
            Debug.Log("🔗 连接状态: " + (status ? "成功 ✅" : "失败 ❌"));
        }

        private void AcquisitionStarted(bool success, bool exceptionRaised, string msg)
        {
            _isAcquisitionRunning = success;
            Debug.Log($"🎬 采集状态: {(success ? "成功 ✅" : "失败 ❌")}，异常: {exceptionRaised}，信息: {msg}");

            if (!success && exceptionRaised) Debug.LogError("❗ 采集过程中出现异常: " + msg);
        }

        private void OnDataReceived(int nSeq, int[] data)
        {
            // ========== 判定区域 ==========

            var wristCanvasPos = GetWristLandmarkCanvasPosition();

            // 你用的全局参数
            var arcCenter = GlobalText.circleCenter;
            var arcStart = GlobalText.circleBottom;
            var arcEnd = GlobalText.circleMid;
            var arcBaseRadius = GlobalText.circleRadius;
            var innerR = arcBaseRadius + mover.arcInnerOffset;
            var outerR = arcBaseRadius + mover.arcOuterOffset;
            var angleStart = Mathf.Atan2(arcStart.y - arcCenter.y, arcStart.x - arcCenter.x);
            var angleEnd = Mathf.Atan2(arcEnd.y - arcCenter.y, arcEnd.x - arcCenter.x);

            // 判定
            var arcZone = ArcBandMathChecker.GetArcZone(
                wristCanvasPos, arcCenter, innerR, outerR, angleStart, angleEnd
            );

            var arcStateForCsv = 0;
            switch (arcZone)
            {
                case ArcBandMathChecker.ArcZone.Inner: arcStateForCsv = -1; break;
                case ArcBandMathChecker.ArcZone.Outer: arcStateForCsv = 1; break;
                case ArcBandMathChecker.ArcZone.Between: arcStateForCsv = 0; break;
            }

            var state = checker != null
                ? checker.JudgeBandPosition(wristCanvasPos)
                : BandCollisionState.Inside; // 没连checker时默认Inside

            var movePhase = (int)mover.GetCurrentMovePhase(); // 0=Moving, 1=Waiting

            // ========== 写入 ==========
            // 这里 arcStateForCsv 就是你要的新状态
            _csvLogger.Write(nSeq, data, arcStateForCsv, (int)state, movePhase);

            _dataProcessor.Process(data);
        }

        // 关键：当前帧手腕canvas位置（和判定时写入同步！）
        private Vector2 GetWristLandmarkCanvasPosition()
        {
            if (tracker == null || targetCanvas == null)
                return Vector2.zero;
            Vector2 wristCanvasPos;
            var wristLandmarkIndex = 15; // 右手腕，左手可用16
            var success = tracker.TryGetCanvasLandmarkPosition(wristLandmarkIndex, targetCanvas, out wristCanvasPos);
            return success ? wristCanvasPos : Vector2.zero;
        }

        private void OnEventDetected(PluxDeviceManager.PluxEvent e)
        {
            Debug.Log("🧭 事件: " + e.type);
        }

        private void OnExceptionRaised(int code, string desc)
        {
            Debug.LogError($"⚠️ 异常 [{code}]: {desc}");
        }

        // ✅ 多通道 UI 更新
        private void UpdateUIWithSmoothedValue(int channelIndex, float value)
        {
            if (channelIndex < muscleCircleControllers.Length && muscleCircleControllers[channelIndex] != null)
                muscleCircleControllers[channelIndex].SetValue(value);
        }
    }
}
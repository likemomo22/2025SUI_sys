using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using utils;
using Utils;

namespace PluxController
{
    public class PluxUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject popupPanel;
        public Button openPopupButton;
        public Button closePopupButton;

        public Button scanButton;
        public Button connectButton;
        public Button startButton;
        public Button stopButton;
        public Button disconnectButton;

        private CsvLogger _csvLogger = new CsvLogger();

        // ✅ 多通道数据处理器（初始化时设置通道数）
        private PluxDataProcessor _dataProcessor;

        // ✅ 多个 UI 圆圈控制器（在 Inspector 中设置）
        public MuscleCircleController.MuscleCircleController[] muscleCircleControllers;

        private PluxDeviceManager _pluxManager;
        private string _selectedMac = "";

        private bool _isConnected = false;
        private bool _isAcquisitionRunning = false;

        [Obsolete("Obsolete")]
        void Start()
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

        void OpenPopup() => popupPanel.SetActive(true);
        void ClosePopup() => popupPanel.SetActive(false);

        [Obsolete("Obsolete")]
        void OnScanClick()
        {
            Debug.Log("开始扫描设备...");
            _pluxManager.GetDetectableDevicesUnity(new List<string> { "BTH" });
        }

        void OnConnectClick()
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

        void OnStartClick()
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
                _pluxManager.StartAcquisitionUnity(100, new List<int> { 1, 2, 3 }, 16); // 根据需要设置通道编号
            }
            catch (System.Exception ex)
            {
                Debug.LogError("采集启动失败: " + ex.Message);
            }
        }

        void OnStopClick()
        {
            if (!_isAcquisitionRunning)
            {
                Debug.LogWarning("⚠️ 当前没有正在进行的采集。");
                return;
            }

            bool result = _pluxManager.StopAcquisitionUnity();
            Debug.Log("⏹ 采集已停止（是否强制）: " + result);
            _isAcquisitionRunning = false;
            _csvLogger.FinalizeLog();
        }

        void OnDisconnectClick()
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

        void ScanResults(List<string> listDevices)
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

        void ConnectionDone(bool status)
        {
            _isConnected = status;
            Debug.Log("🔗 连接状态: " + (status ? "成功 ✅" : "失败 ❌"));
        }

        void AcquisitionStarted(bool success, bool exceptionRaised, string msg)
        {
            _isAcquisitionRunning = success;
            Debug.Log($"🎬 采集状态: {(success ? "成功 ✅" : "失败 ❌")}，异常: {exceptionRaised}，信息: {msg}");

            if (!success && exceptionRaised)
            {
                Debug.LogError("❗ 采集过程中出现异常: " + msg);
            }
        }

        void OnDataReceived(int nSeq, int[] data)
        {
            _csvLogger.Write(nSeq, data);
            _dataProcessor.Process(data);
        }

        void OnEventDetected(PluxDeviceManager.PluxEvent e)
        {
            Debug.Log("🧭 事件: " + e.type);
        }

        void OnExceptionRaised(int code, string desc)
        {
            Debug.LogError($"⚠️ 异常 [{code}]: {desc}");
        }

        // ✅ 多通道 UI 更新
        void UpdateUIWithSmoothedValue(int channelIndex, float value)
        {
            if (channelIndex < muscleCircleControllers.Length && muscleCircleControllers[channelIndex] != null)
            {
                muscleCircleControllers[channelIndex].SetValue(value);
            }
        }
    }
}

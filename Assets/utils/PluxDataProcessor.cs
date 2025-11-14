using System;
using System.Collections.Generic;
using UnityEngine;
using utils;

namespace Utils
{
    /// <summary>
    /// Plux 肌电数据处理器：
    /// 用于对多通道肌电原始数据进行平滑处理，并触发平滑后的数据回调。
    /// 内部通过多个 ChannelProcessor 来分别处理每个通道的数据。
    /// </summary>
    public class PluxDataProcessor
    {
        // 存储每个通道对应的处理器
        private readonly List<ChannelProcessor> channelProcessors = new();

        /// <summary>
        /// 构造函数
        /// channelCount: 通道数量（如 Plux 设备有几个肌电通道）
        /// windowSize: 平滑窗口大小（用于短时间平均）
        /// alpha: 指数平滑系数（越大越敏感，越小越平滑）
        /// </summary>
        public PluxDataProcessor(int channelCount, int windowSize = 100, float alpha = 0.008f)
        {
            for (var i = 0; i < channelCount; i++)
            {
                // 为每个通道创建独立处理器
                var processor = new ChannelProcessor(windowSize, alpha);
                var channelIndex = i;

                // 订阅平滑事件：当通道计算出平滑值后触发
                processor.OnValueSmoothed += smoothed =>
                {
                    // 将通道索引 + 平滑值，通过事件往外通知
                    OnSmoothedValueChanged?.Invoke(channelIndex, smoothed);
                };

                channelProcessors.Add(processor);
            }
        }

        // 通道数据平滑值更新事件
        // int: 通道索引
        // float: 平滑后的值
        public event Action<int, float> OnSmoothedValueChanged;

        /// <summary>
        /// 外部传入原始肌电数据（int 数组）
        /// 每次调用代表一次采样帧，数组长度对应通道数量
        /// </summary>
        public void Process(int[] rawData)
        {
            // 数据为空保护
            if (rawData == null || rawData.Length == 0) return;

            // 遍历每个通道的原始数据
            for (var i = 0; i < rawData.Length && i < channelProcessors.Count; i++)
                // Plux 原始值默认基准（无信号）为 32768，因此需过滤
                if (rawData[i] > 32768)
                {
                    // 去基准偏移，获得真实肌电幅值
                    var fixedValue = rawData[i] - 32768;

                    // 如果当前实验类型为 2 且不是通道1（即 i!=0）
                    // 对除通道1外的值进行系数放大（用于代偿肌的强调显示）
                    if (GlobalText.examType == "2" && i != 0)
                        fixedValue = Mathf.RoundToInt(fixedValue * GlobalText.exaggerateRate); // 注：这里目前是1.0倍（可调整）
                    Debug.Log(fixedValue);
                    // 加入通道处理器进行平滑计算
                    channelProcessors[i].AddValue(fixedValue);
                }
        }

        /// <summary>
        /// 重置所有通道的缓存与状态
        /// 用于重新开始实验或清空历史数据
        /// </summary>
        public void Reset()
        {
            foreach (var processor in channelProcessors)
                processor.Reset();
        }
    }
}

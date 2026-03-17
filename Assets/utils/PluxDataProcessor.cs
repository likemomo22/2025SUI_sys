using System;
using System.Collections.Generic;
using UnityEngine;
using utils;

namespace Utils
{
    /// <summary>
    ///     Plux 多通道肌电处理器：
    ///     负责对原始 EMG 数据进行：
    ///     1) 去基线
    ///     2) 全波整流
    ///     3) （可选）夸张增强
    ///     4) RMS + EMA 平滑
    ///     并通过事件向外部输出每个通道的平滑值。
    /// </summary>
    public class PluxDataProcessor
    {
        private readonly List<ChannelProcessor> _channelProcessors = new();

        /// <summary>
        ///     初始化多通道肌电处理器
        /// 这里是默认值
        /// </summary>
        public PluxDataProcessor(int channelCount, int windowSize = 100, float alpha = 0.005f)
        // public PluxDataProcessor(int channelCount, int windowSize = 6, float alpha = 0.5f)
        {
            for (var i = 0; i < channelCount; i++)
            {
                var processor = new ChannelProcessor(windowSize, alpha);
                var channelIndex = i;

                // 订阅：当某个通道得出平滑值时触发
                processor.OnValueSmoothed += smoothedValue =>
                {
                    OnSmoothedValueChanged?.Invoke(channelIndex, smoothedValue);
                };

                _channelProcessors.Add(processor);
            }
        }

        /// <summary>
        ///     对外输出通道平滑后的肌电值
        ///     int: 通道索引
        ///     float: 平滑后的值
        /// </summary>
        public event Action<int, float> OnSmoothedValueChanged;

        /// <summary>
        ///     传入一次原始肌电数据（对应本次采样的所有通道）
        /// </summary>
        public void Process(int[] rawData)
        {
            if (rawData == null || rawData.Length == 0)
                return;

            var channelCount = Math.Min(rawData.Length, _channelProcessors.Count);

            for (var i = 0; i < channelCount; i++)
            {
                // --------------------
                // 1) 去基线：Plux基线为32768
                // --------------------
                var diff = rawData[i] - 32768;

                // --------------------
                // 2) 全波整流
                // --------------------
                var amp = Mathf.Abs(diff);

                // --------------------
                // 3) 对代偿肌进行强度夸张（可调）
                // --------------------
                if (i != 0)
                    amp = Mathf.RoundToInt(amp * GlobalText.exaggerateRate);

                // --------------------
                // 4) 丢入通道处理器（RMS + EMA）
                // --------------------
                _channelProcessors[i].AddValue(amp);
            }
        }

        /// <summary>
        ///     清空所有通道历史数据（重新开始实验时调用）
        /// </summary>
        public void Reset()
        {
            foreach (var p in _channelProcessors)
                p.Reset();
        }
    }
}
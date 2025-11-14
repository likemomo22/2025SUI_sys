using System;
using System.Collections.Generic;
using UnityEngine;

namespace utils
{
    /// <summary>
    /// 单个通道的肌电数据处理器：
    /// 使用窗口RMS + 自适应指数平滑，输出平稳的肌肉激活强度。
    /// </summary>
    public class ChannelProcessor
    {
        private readonly float alpha;                     // 基础平滑系数
        private readonly Queue<int> window = new();       // 滑动窗口
        private readonly int windowSize;                  // 窗口大小
        private float smoothedValue;                      // 平滑结果

        public ChannelProcessor(int windowSize = 80, float alpha = 0.1f)//不是在这里设置，在PluxDataProcessor.cs
        {
            this.windowSize = windowSize;
            this.alpha = alpha;
        }

        public event Action<float> OnValueSmoothed;       // 输出回调事件

        public void AddValue(int value)
        {
            // 新数据入队，保持窗口长度
            window.Enqueue(value);
            if (window.Count > windowSize)
                window.Dequeue();

            // === 计算窗口 RMS ===
            var sumSq = 0f;
            foreach (var val in window)
                sumSq += val * val;

            var rms = (float)Math.Sqrt(sumSq / window.Count);
            Debug.Log(rms);

            // === 动态调整 alpha ===
            // 上升快，下降慢：适合肌电视觉化
            var dynamicAlpha = rms > smoothedValue ? alpha * 1.5f : alpha * 0.2f;
            dynamicAlpha = Math.Clamp(dynamicAlpha, 0.01f, 1.0f);

            // === 指数平滑 EWMA ===
            smoothedValue = dynamicAlpha * rms + (1 - dynamicAlpha) * smoothedValue;

            // 输出
            OnValueSmoothed?.Invoke(smoothedValue);
        }

        public void Reset()
        {
            smoothedValue = 0f;
            window.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace utils
{
    /// <summary>
    ///     单通道肌电处理器：
    ///     RMS 包络 + 自适应 EMA（上升快 / 下降慢 / 稳态强抑制）
    ///     解决稳态保持时的抖动问题。
    /// </summary>
    public class ChannelProcessor
    {
        private readonly float baseAlpha; // EMA 基础系数
        private readonly float steadyThresholdRatio; // 稳态死区比例（相对值）

        private readonly Queue<int> window = new(); // RMS 滑动窗口
        private readonly int windowSize; // RMS 窗口大小（samples）
        private float smoothedValue; // 当前平滑值

        /// <param name="windowSize">RMS 窗口大小（1000Hz 下推荐 100~120）</param>
        /// <param name="alpha">EMA 基础系数（推荐 0.06~0.08）</param>
        /// <param name="steadyRatio">稳态死区比例（推荐 0.02~0.04）</param>
        public ChannelProcessor(
            int windowSize = 1000,
            float alpha = 0.08f,
            float steadyRatio = 0.05f)

            // -------------AH--------
            // int windowSize = 12, // ≈ 200 ms RMS
            // float alpha = 0.20f, // EMA 时间常数 ≈ 150–200 ms
            // float steadyRatio = 0.05f // 5% 稳态死区
        // )

        {
            this.windowSize = windowSize;
            baseAlpha = alpha;
            steadyThresholdRatio = steadyRatio;
        }

        public event Action<float> OnValueSmoothed;

        /// <summary>
        ///     输入一帧整流后的 EMG 幅值
        /// </summary>
        public void AddValue(int value)
        {
            // ---------- RMS 窗口 ----------
            window.Enqueue(value);
            if (window.Count > windowSize)
                window.Dequeue();

            var sumSq = 0f;
            foreach (var v in window)
                sumSq += v * v;

            var rms = Mathf.Sqrt(sumSq / window.Count);

            // ---------- 自适应 EMA（含稳态死区） ----------
            var diff = Mathf.Abs(rms - smoothedValue);
            var threshold = smoothedValue * steadyThresholdRatio;

            float dynamicAlpha;

            if (smoothedValue > 0f && diff < threshold)
                // ===== 稳态保持区：极强抑制（防抖）=====
                dynamicAlpha = baseAlpha * 0.5f;
            else if (rms > smoothedValue)
                // ===== 明确上升（用力）=====
                dynamicAlpha = baseAlpha * 1.2f;
            else
                // ===== 明确下降（放松）=====
                dynamicAlpha = baseAlpha * 0.1f;

            dynamicAlpha = Mathf.Clamp(dynamicAlpha, 0.005f, 1f);

            //-------------AH-------------
            // dynamicAlpha = Mathf.Clamp(dynamicAlpha, 0.05f, 0.4f);



            // ---------- EMA ----------
            smoothedValue =
                dynamicAlpha * rms +
                (1f - dynamicAlpha) * smoothedValue;

            OnValueSmoothed?.Invoke(smoothedValue);
        }

        /// <summary>
        ///     重置（实验重新开始时调用）
        /// </summary>
        public void Reset()
        {
            smoothedValue = 0f;
            window.Clear();
        }
    }
}
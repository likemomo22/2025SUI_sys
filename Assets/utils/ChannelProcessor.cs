using System;
using System.Collections.Generic;

namespace utils
{
    public class ChannelProcessor
    {
        private Queue<int> window = new Queue<int>();
        private int windowSize;
        private float alpha;
        private float smoothedValue = 0f;

        public event Action<float> OnValueSmoothed;

        public ChannelProcessor(int windowSize = 20, float alpha = 0.1f)
        {
            this.windowSize = windowSize;
            this.alpha = alpha;
        }

        public void AddValue(int value)
        {
            window.Enqueue(value);
            if (window.Count > windowSize)
                window.Dequeue();

            float avg = 0f;
            foreach (var val in window)
                avg += val;
            avg /= window.Count;

            // 动态调整 alpha：上升快，下降慢
            float dynamicAlpha = avg > smoothedValue ? alpha * 1.5f : alpha * 0.2f;
            dynamicAlpha = Math.Clamp(dynamicAlpha, 0.01f, 1.0f);  // 避免超出范围

            smoothedValue = dynamicAlpha * avg + (1 - dynamicAlpha) * smoothedValue;
            OnValueSmoothed?.Invoke(smoothedValue);
        }


        public void Reset()
        {
            smoothedValue = 0f;
            window.Clear();
        }
    }
}
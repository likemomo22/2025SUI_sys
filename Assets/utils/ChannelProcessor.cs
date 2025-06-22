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

        public ChannelProcessor(int windowSize = 10, float alpha = 0.2f)
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

            smoothedValue = alpha * avg + (1 - alpha) * smoothedValue;
            OnValueSmoothed?.Invoke(smoothedValue);
        }

        public void Reset()
        {
            smoothedValue = 0f;
            window.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using utils;

namespace Utils
{
    public class PluxDataProcessor
    {
        private List<ChannelProcessor> channelProcessors = new List<ChannelProcessor>();

        public event Action<int, float> OnSmoothedValueChanged; // int: channel index

        public PluxDataProcessor(int channelCount, int windowSize = 10, float alpha = 0.15f)
        {
            for (int i = 0; i < channelCount; i++)
            {
                var processor = new ChannelProcessor(windowSize, alpha);
                int channelIndex = i;
                processor.OnValueSmoothed += (smoothed) =>
                {
                    OnSmoothedValueChanged?.Invoke(channelIndex, smoothed);
                };
                channelProcessors.Add(processor);
            }
        }

        public void Process(int[] rawData)
        {
            if (rawData == null || rawData.Length == 0) return;

            for (int i = 0; i < rawData.Length && i < channelProcessors.Count; i++)
            {
                if (rawData[i] > 32768)
                {
                    int fixedValue = rawData[i] - 32768;

                    //  type==2 且不是channel 1时（即i != 0），放大 1.25 倍
                    if (GlobalText.ExamType == "2" && i != 0)
                    {
                        fixedValue = Mathf.RoundToInt(fixedValue * 2.0f);
                    }

                    channelProcessors[i].AddValue(fixedValue);
                }
            }
        }


        public void Reset()
        {
            foreach (var processor in channelProcessors)
                processor.Reset();
        }
    }
}
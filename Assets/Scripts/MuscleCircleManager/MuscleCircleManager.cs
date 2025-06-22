using UnityEngine;
using MuscleCircleController;
using Utils;

public class MuscleCircleManager : MonoBehaviour
{
    public MuscleCircleController.MuscleCircleController[] controllers; // 挂载在 Inspector

    private PluxDataProcessor processor;

    void Start()
    {
        // 初始化 3 通道处理器
        processor = new PluxDataProcessor(channelCount: 3);
        processor.OnSmoothedValueChanged += OnSmoothedValue;
    }

    void OnSmoothedValue(int channelIndex, float value)
    {
        if (channelIndex < controllers.Length && controllers[channelIndex] != null)
        {
            controllers[channelIndex].SetValue(value);
        }
    }

    // 从外部传入 rawData，通常在 Update() 或外部调用
    public void ProcessRawData(int[] rawData)
    {
        processor.Process(rawData);
    }

    public void ResetAll()
    {
        processor.Reset();
    }
}
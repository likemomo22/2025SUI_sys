using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace utils
{
    public class CsvLogger
    {
        private readonly List<string> buffer = new();
        private string filePath = "";

        // 新增：支持判定
        public void Init()
        {
            var fileName = $"UserId_{GlobalText.userId}___Type_{GlobalText.examType}.csv";
            filePath = Path.Combine(@"C:\Users\munek\2025SUI_ver4_data", fileName);
            buffer.Clear();
            buffer.Add("Frame,Channel1,Channel2,Channel3,arcStateForCsv,JudgeState,movePhase"); // 多一列状态
        }

        // 新增参数 judgeState
        public void Write(int frame, int[] rawData, int arcStateForCsv, int judgeState, int movePhase)
        {
            var line = $"{frame},{string.Join(",", rawData)},{arcStateForCsv},{judgeState},{movePhase}";
            buffer.Add(line);
            try
            {
                File.AppendAllLines(filePath, new List<string> { line });
            }
            catch (IOException ex)
            {
                Debug.LogError("CSV写入错误: " + ex.Message);
            }
        }

        public void FinalizeLog()
        {
            File.WriteAllLines(filePath, buffer);
        }
    }
}
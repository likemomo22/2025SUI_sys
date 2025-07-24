using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace utils
{
    public class CsvLogger
    {
        private List<string> buffer = new List<string>();
        private string filePath = "";

        public void Init()
        {
            string fileName = $"UserId_{GlobalText.UserId}___Type_{GlobalText.ExamType}.csv";
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            buffer.Clear();
            buffer.Add("Frame,Channel1,Channel2,Channel3"); // 可扩展为多通道
        }

        public void Write(int frame, int[] rawData)
        {
            string line = $"{frame},{string.Join(",", rawData)}";
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
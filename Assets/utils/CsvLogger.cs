using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;


namespace utils
{
    public class CsvLogger
    {
        private readonly List<string> buffer = new();
        private string filePath = "";

        /// <summary>
        /// 初始化 CSV 文件
        /// </summary>
        public void Init()
        {
            var fileName = $"UserId_{GlobalText.userId}___Type_{GlobalText.examType}.csv";
            filePath = Path.Combine(@"C:\Users\munek\2025SUI_ver4_data", fileName);
            buffer.Clear();

            // ✅ 新增 colorState 列
            buffer.Add(
                "Frame,Channel1,Channel2,Channel3," +
                "arcStateForCsv,JudgeState,movePhase,targetColorState,subColorState,pcTimestamp,"
            );


        }

        /// <summary>
        /// 写入一行数据
        /// colorState: 0=绿, 1=黄, 2=红
        /// </summary>
        public void Write(
            int frame,
            int[] rawData,
            int arcStateForCsv,
            int judgeState,
            int movePhase,
            int targetColorState,
            int subColorState
        )
        {
            // ✅ 当前电脑真实时间
            string pcTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var line =
                $"{frame},"  +
                $"{string.Join(",", rawData)}," +
                $"{arcStateForCsv}," +
                $"{judgeState}," +
                $"{movePhase}," +
                $"{targetColorState}," +
                $"{subColorState}"+
                $"{pcTimestamp},";

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


        /// <summary>
        /// 实验结束后统一写入（兜底）
        /// </summary>
        public void FinalizeLog()
        {
            File.WriteAllLines(filePath, buffer);
        }
    }
}
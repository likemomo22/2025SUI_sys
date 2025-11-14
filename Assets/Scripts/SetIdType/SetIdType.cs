using UnityEngine;
using UnityEngine.UI;
using utils;

namespace SetIdType
{
    public class SetIdType : MonoBehaviour
    {
        public Button inputIdButton;
        public Text userIdText;

        public Button inputTypeButton;
        public Text typeText;

        public Button confirmButton;
        public Text maxValueTimesText;

        public Button exaggerateRateButton;
        public Text exaggerateRateText;

        private void Start()
        {
            inputIdButton.onClick.AddListener(OnInputIdButtonClick);
            inputTypeButton.onClick.AddListener(OnInputTypeButtonClick);
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            exaggerateRateButton.onClick.AddListener(OnExaggerateRateButtonClick);
        }

        private void OnInputIdButtonClick()
        {
            var userId = userIdText.text;
            GlobalText.userId = userId;
        }

        private void OnInputTypeButtonClick()
        {
            var examType = typeText.text;
            GlobalText.examType = examType;
        }

        private void OnConfirmButtonClick()
        {
            var maxValueTimes = maxValueTimesText.text;
            GlobalText.maxValueTimes = maxValueTimes;
        }

        private void OnExaggerateRateButtonClick()
        {
            if (float.TryParse(exaggerateRateText.text, out var exaggerateRate))
                GlobalText.exaggerateRate = exaggerateRate;
            else
                Debug.LogError("输入的 exaggerateRate 不是有效数字！");
        }
    }
}
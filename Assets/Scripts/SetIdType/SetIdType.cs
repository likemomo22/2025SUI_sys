using UnityEngine;
using UnityEngine.UI;
using utils;

namespace SetIdType
{
    public class SetIdType:MonoBehaviour
    {
        public Button inputIdButton;
        public Text userIdText;

        public Button inputTypeButton;
        public Text typeText;

        private void Start()
        {
            inputIdButton.onClick.AddListener(OnInputIdButtonClick);
            inputTypeButton.onClick.AddListener(OnInputTypeButtonClick);
        }

        void OnInputIdButtonClick()
        {
            string userId = userIdText.text;
            GlobalText.UserId = userId;
        }
        void OnInputTypeButtonClick()
        {
            string examType = typeText.text;
            GlobalText.ExamType = examType;
        }
    }
}
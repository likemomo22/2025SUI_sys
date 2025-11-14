using UnityEngine;
using UnityEngine.UI;

namespace utils
{
    public class testGlobal : MonoBehaviour

    {
        public Button testButton;

        private void Start()
        {
            testButton.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            Debug.Log(GlobalText.userId);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace utils
{
    public class SetExaggrateRate:MonoBehaviour
    {
        public float ExaggerateRate = GlobalText.exaggerateRate;
        
        public Button SetExaggerateRateButton;
        public Text ExaggerateRateText;
        
        private void Start()
        {
            SetExaggerateRateButton.onClick.AddListener(OnSetExaggerateRateButtonClick);
        }
        
        private void OnSetExaggerateRateButtonClick()
        {
            if (float.TryParse(ExaggerateRateText.text, out var exaggerateRate))
                GlobalText.exaggerateRate = exaggerateRate;
            else
                UnityEngine.Debug.LogError("输入的 exaggerateRate 不是有效数字！");
        }
    }
}
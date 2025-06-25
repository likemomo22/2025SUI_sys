using UnityEngine;
using UnityEngine.UI;



namespace utils
{
    public class testGlobal:MonoBehaviour

    {
    public Button testButton;

    void Start()
    {
        testButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        Debug.Log(GlobalText.UserId);
    }
    }
}
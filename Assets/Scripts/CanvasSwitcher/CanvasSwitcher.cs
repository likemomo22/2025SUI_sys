using System;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasSwitcher
{
    public class CanvasSwitcher:MonoBehaviour

    {
        [Header("Canvas")]
        public Canvas homeCanvas;
        public Canvas mirrorCanvas;
        public Canvas mainCanvas;
        
        [Header("Button")]
        public Button start1Button;
        public Button start2Button;

        private void Awake()
        {
            mirrorCanvas.gameObject.SetActive(false);
            mainCanvas.gameObject.SetActive(false);

        }

        void Start()
        {
            start1Button.onClick.AddListener(SwitchToType0Canvas);
            start2Button.onClick.AddListener(SwitchToType1Canvas);
        }
        void SwitchToType0Canvas()
        {
            homeCanvas.gameObject.SetActive(false);
            mirrorCanvas.gameObject.SetActive(true);
        }
        
        void SwitchToType1Canvas()
        {
            homeCanvas.gameObject.SetActive(false);
            mainCanvas.gameObject.SetActive(true);
        }
    }
    
}
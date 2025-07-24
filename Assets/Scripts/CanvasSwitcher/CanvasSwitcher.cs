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
        public Canvas mainCanvasVer2;
        
        [Header("Button")]
        public Button start1Button;
        public Button start2Button;
        public Button start3Button;

        private void Awake()
        {
            mirrorCanvas.gameObject.SetActive(false);
            mainCanvas.gameObject.SetActive(false);
            mainCanvasVer2.gameObject.SetActive(false);

        }

        void Start()
        {
            start1Button.onClick.AddListener(SwitchToType0Canvas);
            start2Button.onClick.AddListener(SwitchToType1Canvas);
            start3Button.onClick.AddListener(SwitchToType2Canvas);
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
        private void SwitchToType2Canvas()
        {
            homeCanvas.gameObject.SetActive(false);
            mainCanvasVer2.gameObject.SetActive(true);
        }

    }
    
}
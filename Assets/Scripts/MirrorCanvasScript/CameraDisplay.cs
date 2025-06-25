using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorCanvasScript
{
    public class CameraDisplay: MonoBehaviour
    {
        public RawImage rawImage;
        private WebCamTexture _webCamTexture;

        void Start()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                Debug.Log("VAR");
                string camName = devices[0].name;
                _webCamTexture = new WebCamTexture(camName, 1280, 720, 30);
                rawImage.texture = _webCamTexture;
                rawImage.material.mainTexture = _webCamTexture;
                _webCamTexture.Play();
                Debug.Log("2");

                rawImage.rectTransform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                Debug.LogWarning("未检测到摄像头设备");
            }
        }

        private void OnDestroy()
        {
            if (_webCamTexture != null && _webCamTexture.isPlaying)
            {
                _webCamTexture.Stop();
            }
        }
    }
}
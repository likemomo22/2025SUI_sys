using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainCanvasver2.UIControl.CircleVisible
{
    public class SetCircleVisible:MonoBehaviour
    {
        public Button toggleButton;    
        public MuscleCircleController.MuscleCircleController muscleCircleController;
        
        private float _originalValue;
        [SerializeField] private bool isZeroed = false;

        private void Start()
        {
            toggleButton.onClick.AddListener(OnToggleButtonClick);
        }

        private void OnToggleButtonClick()
        {
            if (!isZeroed)
            {
                _originalValue = muscleCircleController.maxValue;
                muscleCircleController.maxValue = 0f;
                isZeroed = true;
            }
            else
            {
                muscleCircleController.maxValue = _originalValue;
                isZeroed = false;
            }
        }
    }
}

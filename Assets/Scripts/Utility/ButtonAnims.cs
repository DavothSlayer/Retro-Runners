using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RetroCode
{
    public class ButtonAnims : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rect;

        [SerializeField]
        private Vector3 defaultSize;
        [SerializeField]
        private Vector3 scaleOffset;
        [SerializeField]
        private float lerpTime;

        private Vector3 targetScale;

        private void OnEnable()
        {
            targetScale = defaultSize;
        }

        private void Update()
        {
            targetScale = Vector3.Lerp(targetScale, defaultSize, Time.deltaTime / lerpTime);

            rect.localScale = targetScale;
        }

        public void ScaleButton()
        {
            targetScale += scaleOffset;
        }
    }
}

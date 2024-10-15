using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class ScreenInteract : MonoBehaviour
    {
        [SerializeField]
        private RetroGarage retroGarage;
        [SerializeField]
        private float touchSensitivity;

        private Vector2 touchStartPos;
        private Vector2 touchInput;

        private bool active = true;

        [HideInInspector]
        public float inputX;

        private void Update()
        {
            if (Input.touchCount == 0)
            {
                inputX = Mathf.Lerp(inputX, 0f, Time.deltaTime);
                return;
            }

            if (!active) return;

            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    touchInput = Vector2.zero;
                    touchStartPos = Input.GetTouch(0).position;
                    break;
                case TouchPhase.Moved:
                    touchInput = Input.GetTouch(0).position - touchStartPos;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    touchInput = Vector2.zero;
                    touchStartPos = Vector2.zero;
                    break;
            }

            inputX = Mathf.Lerp(inputX, touchInput.x, 6f * Time.deltaTime);
        }

        public void SetSwipeState(bool state)
        {
            active = state;
            print($"SwipeState: {active}");
        }
    }
}

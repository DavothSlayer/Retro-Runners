using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class SwipeSelect : MonoBehaviour
    {
        [SerializeField]
        private RetroGarage retroGarage;
        [SerializeField]
        private float touchSensitivity;
        [SerializeField]
        private AudioClip swipeSound;

        private Vector2 touchStartPos;
        private Vector2 touchEndPos;

        private bool active = true;

        private void Update()
        {
            if(Input.touchCount == 0) return;
            if (!active) return;

            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    touchStartPos = Input.GetTouch(0).position;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    touchEndPos = Input.GetTouch(0).position;

                    if (touchEndPos.x > touchStartPos.x + touchSensitivity)
                    {
                        retroGarage.SwipeNext();

                        retroGarage.audioSource.PlayOneShot(swipeSound);
                    }

                    if (touchEndPos.x < touchStartPos.x - touchSensitivity)
                    {
                        retroGarage.SwipePrevious();

                        retroGarage.audioSource.PlayOneShot(swipeSound);
                    }
                    break;
            }
        }

        public void SetSwipeState(bool state)
        {
            active = state;
            print($"SwipeState: {active}");
        }
    }
}

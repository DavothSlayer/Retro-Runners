using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [HideInInspector]
    public float xTouch;

    [HideInInspector]
    public float xTouchLerp;

    public delegate void UpperDoubleTap();
    public static event UpperDoubleTap UpperDoubleTapped;

    public delegate void LowerDoubleTap();
    public static event LowerDoubleTap LowerDoubleTapped;

    private void Update()
    {
#if UNITY_ANDROID
        MobileInput();
#endif

#if UNITY_EDITOR
        PCInput();
#endif
    }

    #region PC Input
    private void PCInput()
    {
        xTouch = Input.GetAxis("Horizontal");
        xTouchLerp = Mathf.Lerp(xTouchLerp, xTouch, 1.5f * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.W))
            UpperDoubleTapped?.Invoke();

        if (Input.GetKeyDown(KeyCode.S))
            LowerDoubleTapped?.Invoke();
    }
    #endregion

    #region Mobile Input
    private Vector2 touchStartPosition;
    private Touch theTouch;
    private void MobileInput()
    {
        xTouchLerp = Mathf.Lerp(xTouchLerp, xTouch, 1.5f * Time.deltaTime);

        if (Input.touchCount == 0) { return; }
        else { theTouch = Input.GetTouch(0); }

        switch (theTouch.phase)
        {
            case TouchPhase.Began:

                touchStartPosition = theTouch.position;

                if(touchStartPosition.x > Screen.currentResolution.width * 0.66f)
                    xTouch = 1f;

                if (touchStartPosition.x < Screen.currentResolution.width * 0.33f)
                    xTouch = -1f;

                if (touchStartPosition.x > Screen.currentResolution.width * 0.33f && 
                    touchStartPosition.x < Screen.currentResolution.width * 0.66f && 
                    touchStartPosition.y > Screen.currentResolution.height * 0.5f)
                    if (theTouch.tapCount >= 2)
                        UpperDoubleTapped?.Invoke();

                if (touchStartPosition.x > Screen.currentResolution.width * 0.33f &&
                    touchStartPosition.x < Screen.currentResolution.width * 0.66f &&
                    touchStartPosition.y < Screen.currentResolution.height * 0.5f)
                    if (theTouch.tapCount >= 2)
                        LowerDoubleTapped?.Invoke();


                    break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:

                xTouch = 0f;

                break;
        }
    }
    #endregion Mobile Input
}

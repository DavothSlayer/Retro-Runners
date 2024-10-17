using TMPro;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    public void ButtonPressBounce(GameObject gameObject)
    {
        LeanTween.scale(gameObject, new(1.15f, 1.15f, 1f), .1f).setEaseInBounce();
        LeanTween.scale(gameObject, Vector3.one, .1f).setDelay(.1f);
    }

    public void BlinkFadeOut(TextMeshProUGUI text)
    {
        LeanTween.value(gameObject, text.alpha, .1f, .5f)
            .setOnUpdate((float value) =>
            {
                // Get the current color
                Color color = text.color;

                // Set the alpha value
                color.a = value;

                // Apply the new color to the TextMeshProUGUI component
                text.color = color;
            })
            .setOnComplete(() => BlinkFadeIn(text));
    }

    public void BlinkFadeIn(TextMeshProUGUI text)
    {
        LeanTween.value(gameObject, text.alpha, 1f, .5f)
            .setOnUpdate((float value) =>
            {
                // Get the current color
                Color color = text.color;

                // Set the alpha value
                color.a = value;

                // Apply the new color to the TextMeshProUGUI component
                text.color = color;
            })
            .setOnComplete(() => BlinkFadeOut(text));
    }

    public void ArrowToRight(GameObject gameObject)
    {
        float initX = gameObject.transform.localPosition.x + 10f;
        LeanTween.moveLocalX(gameObject, initX, .35f).setOnComplete(() => ArrowToLeft(gameObject));
    }

    public void ArrowToLeft(GameObject gameObject)
    {
        float initX = gameObject.transform.localPosition.x - 10f;
        LeanTween.moveLocalX(gameObject, initX, .35f).setOnComplete(() => ArrowToRight(gameObject));
    }
}

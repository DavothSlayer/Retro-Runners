using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    public void ButtonPressBounce(GameObject gameObject)
    {
        LeanTween.scale(gameObject, new(1.1f, 1.1f, 1f), .1f).setIgnoreTimeScale(true).setEaseInBounce();
        LeanTween.scale(gameObject, Vector3.one, .1f).setDelay(.1f).setIgnoreTimeScale(true);
    }

    public void BounceContinous(GameObject gameObject)
    {
        LeanTween.scale(gameObject, new(1.1f, 1.1f, 1f), .1f).setEaseInBounce();
        LeanTween.scale(gameObject, Vector3.one, .1f).setDelay(.1f).setOnComplete(() => BounceContinousComplete(gameObject));
    }

    private async void BounceContinousComplete(GameObject gameObject)
    {
        await Task.Delay(750);

        BounceContinous(gameObject);
    }

    public void BlinkFadeOut(TextMeshProUGUI text)
    {
        LeanTween.value(gameObject, text.alpha, .1f, .5f)
            .setOnUpdate((float value) =>
            {
                Color color = text.color;

                color.a = value;

                text.color = color;
            })
            .setOnComplete(() => BlinkFadeIn(text));
    }

    public void BlinkFadeIn(TextMeshProUGUI text)
    {
        LeanTween.value(gameObject, text.alpha, 1f, .5f)
            .setOnUpdate((float value) =>
            {
                Color color = text.color;

                color.a = value;

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

    public void FadeInCanvasGroup(CanvasGroup canvasGroup)
    {
        canvasGroup.gameObject.SetActive(true);
        LeanTween.alphaCanvas(canvasGroup, 1f, .15f).setIgnoreTimeScale(true);
    }

    public void FadeOutCanvasGroup(CanvasGroup canvasGroup)
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, .25f).setIgnoreTimeScale(true).setOnComplete(() => canvasGroup.gameObject.SetActive(false));
    }
}

using UnityEngine;

public class Tweener : MonoBehaviour
{
    public void ButtonPressBounce(GameObject gameObject)
    {
        LeanTween.scale(gameObject, new(1.15f, 1.15f, 1f), .1f).setEaseInBounce();
        LeanTween.scale(gameObject, Vector3.one, .1f).setDelay(.1f);
    }
}

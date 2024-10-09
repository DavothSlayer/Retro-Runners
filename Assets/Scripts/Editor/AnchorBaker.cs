using UnityEngine;
using UnityEditor;

public class AnchorBaker
{
    [MenuItem("CONTEXT/RectTransform/Bake Anchors")]
    static void BakeAnchors(MenuCommand command)
    {
        RectTransform rectTransform = (RectTransform)command.context;

        if (rectTransform != null)
        {
            SetAnchorsToCorners(rectTransform);
            Debug.Log("Anchors baked for " + rectTransform.name);
        }
    }

    public static void SetAnchorsToCorners(RectTransform rectTransform)
    {
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();

        if (parentRect == null)
        {
            Debug.LogWarning("The parent of this RectTransform doesn't have a RectTransform component.");
            return;
        }

        Vector2 newAnchorsMin = new Vector2(
            rectTransform.anchorMin.x + rectTransform.offsetMin.x / parentRect.rect.width,
            rectTransform.anchorMin.y + rectTransform.offsetMin.y / parentRect.rect.height);

        Vector2 newAnchorsMax = new Vector2(
            rectTransform.anchorMax.x + rectTransform.offsetMax.x / parentRect.rect.width,
            rectTransform.anchorMax.y + rectTransform.offsetMax.y / parentRect.rect.height);

        rectTransform.anchorMin = newAnchorsMin;
        rectTransform.anchorMax = newAnchorsMax;

        rectTransform.offsetMin = rectTransform.offsetMax = new Vector2(0, 0);
    }
}

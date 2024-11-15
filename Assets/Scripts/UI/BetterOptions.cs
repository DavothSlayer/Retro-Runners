using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BetterOptions : MonoBehaviour
{
    [Space]    
    public int optionIndex;

    [Header("References")]
    [SerializeField]
    private List<GameObject> optionObjects = new List<GameObject>();

    [Space]
    public UnityEvent valueChanged;

    public void OnValidate()
    {
        foreach (GameObject gameObject in optionObjects)
            gameObject.SetActive(gameObject == optionObjects[optionIndex]);
    }

    public void SetValue(int index)
    {
        optionIndex = index;

        foreach (GameObject gameObject in optionObjects)
            gameObject.SetActive(gameObject == optionObjects[optionIndex]);

        valueChanged?.Invoke();
    }

    public void IterateValue()
    {
        if (optionIndex == optionObjects.Count - 1) optionIndex = 0;
        else { optionIndex++; }

        foreach (GameObject gameObject in optionObjects)
            gameObject.SetActive(gameObject == optionObjects[optionIndex]);

        valueChanged?.Invoke();
    }
}

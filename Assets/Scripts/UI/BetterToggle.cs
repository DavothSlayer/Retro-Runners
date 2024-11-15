using UnityEngine;
using UnityEngine.Events;

public class BetterToggle : MonoBehaviour
{
    [Space]
    public bool isOn;

    [Header("References")]
    [SerializeField]
    private GameObject onObject;
    [SerializeField]
    private GameObject offObject;

    [Space]
    public UnityEvent valueChanged;

    public void OnValidate()
    {
        onObject.SetActive(isOn);
        offObject.SetActive(!isOn);
    }

    public void SetValue(bool value)
    {
        isOn = value;

        onObject.SetActive(isOn);
        offObject.SetActive(!isOn);

        valueChanged?.Invoke();
    }

    public void ToggleValue()
    {
        isOn = !isOn;

        onObject.SetActive(isOn);
        offObject.SetActive(!isOn);

        valueChanged?.Invoke();
    }
}

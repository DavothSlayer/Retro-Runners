using UnityEngine;
using UnityEngine.Events;

public class TweenCaller : MonoBehaviour
{
    public UnityEvent Init;
    public UnityEvent Enable;

    public void Start()
    {
        Init?.Invoke();
    }

    public void OnEnable()
    {
        Enable?.Invoke();
    }
}

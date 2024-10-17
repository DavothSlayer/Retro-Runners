using UnityEngine;
using UnityEngine.Events;

public class TweenCaller : MonoBehaviour
{
    public UnityEvent Init;

    public void Start()
    {
        Init?.Invoke();
    }
}

using UnityEngine;
using UnityEngine.Events;

namespace RetroCode
{
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
}

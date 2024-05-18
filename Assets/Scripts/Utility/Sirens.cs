using UnityEngine;

namespace RetroCode
{
    public class Sirens : MonoBehaviour
    {
        [SerializeField]
        private GameObject sirenVar1;
        [SerializeField]
        private GameObject sirenVar2;

        private void Update() => SirenMethod();

        private float timer;
        private float sirenFrequency = 0.3f;
        private void SirenMethod()
        {
            timer += Time.deltaTime;

            if (timer >= sirenFrequency)
            {
                sirenVar1.SetActive(!sirenVar1.activeInHierarchy);
                sirenVar2.SetActive(!sirenVar2.activeInHierarchy);

                timer = 0f;
            }
        }
    }
}

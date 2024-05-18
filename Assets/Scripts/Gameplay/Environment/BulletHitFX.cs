using UnityEngine;

namespace RetroCode
{
    public class BulletHitFX : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        private void OnEnable() => audioSource.pitch = Random.Range(0.9f, 1.1f);
    }
}

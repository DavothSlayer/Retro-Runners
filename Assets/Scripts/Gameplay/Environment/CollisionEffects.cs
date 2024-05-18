using UnityEngine;

namespace RetroCode
{
    public class CollisionEffects : MonoBehaviour
    {
        [Header("SFX")]
        [SerializeField]
        private AudioClip[] selection;
        [SerializeField]
        private AudioSource soundSource;

        public void OnCollisionEnter(Collision collision)
        {
            if(collision.collider.gameObject.name == "RoadCollider") { return; }

            soundSource.PlayOneShot(selection[Random.Range(0, selection.Length)]);
        }
    }
}

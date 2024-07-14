using System.Collections;
using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class Pickup : MonoBehaviour, PickUp
    {
        [Header("Main")]
        [SerializeField]
        private PickupType pickupType;
        [SerializeField]
        private SpawnManager spawnManager;
        [SerializeField]
        private GameManager gameManager;
        
        [Space]
        [Header("Floating")]
        [SerializeField]
        private Transform hoverable;
        [SerializeField]
        private float rotationSpeed;
        [SerializeField]
        private float hoverHeight;
        [SerializeField]
        private float hoverFreq;
        [SerializeField]
        private float hoverAmp;

        [Space]
        [Header("FX")]
        [SerializeField]
        private ParticleSystem[] mainFX;
        [SerializeField]
        private ParticleSystem[] pickupFX;

        private void Awake()
        {
            hoverable.localPosition += new Vector3(0f, hoverHeight, 0f);
        }

        private void OnEnable()
        {
            hoverable.gameObject.SetActive(true);

            foreach (ParticleSystem ps in mainFX)
                ps.PlaySystem();
        }

        private void Update()
        {
            hoverable.Rotate(rotationSpeed * Time.deltaTime * Vector3.up, Space.World);
            hoverable.localPosition = new Vector3(0f, hoverHeight + Mathf.Sin(Time.time * hoverFreq) * hoverAmp, 0f);

            if (gameManager.playerTransform == null) return;
            if (gameManager.playerTransform.position.z >= transform.position.z + spawnManager.minDespawnDistance) spawnManager.RemovePickup(gameObject);
        }

        public void PickupInit()
        {
            foreach (ParticleSystem ps in mainFX)
                ps.StopSystem();

            foreach (ParticleSystem ps in pickupFX)
                ps.PlaySystem();

            hoverable.gameObject.SetActive(false);
        }

        public PickupType GetPickupType()
        {
            return pickupType;
        }

        public GameObject Hoverable()
        {
            return hoverable.gameObject;
        }
    }

    public enum PickupType
    {
        PickupBonus,
        Boost,
        Fixer,
    }
}

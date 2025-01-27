using Unity.Burst;
using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class NPC : MonoBehaviour, Damageable, NearMissable
    {
        [Header("Managers")]
        [SerializeField]
        private SpawnManager spawnManager;

        [Header("Core")]
        [SerializeField]
        private Rigidbody rb;
        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;        
        public float topSpeed;
        public float targetSpeed;
        public NPCType NPCType;
        [SerializeField]
        private float nearMissProxy;

        [Header("Visual")]
        [SerializeField]
        private Transform[] wheelSets;
        [SerializeField]
        private float wheelDiameter;

        public void Damage(int dmg)
        {
            if (health <= 0) return;

            health -= dmg;

            if(health <= 0) HandleDeath();
        }

        [BurstCompile]
        private void FixedUpdate()
        {
            rb.AddForce(25f * rb.linearDamping * rb.mass * transform.forward, ForceMode.Force);
        }

        [BurstCompile]
        public void Update()
        {
            foreach (Transform wheels in wheelSets)
            {
                wheels.Rotate(Vector3.right, (15f * Time.deltaTime / wheelDiameter * 3.14f) * 360f, Space.Self);
            }
        }

        [BurstCompile]        
        public virtual void HandleDeath()
        {
            GameManager.TriggerDestroyedNPCEvent();

            spawnManager.HandleDeadNPC(this);
        }

        public int Health()
        {
            return health;
        }

        public int DamageToPlayer()
        {
            return damageToPlayer;
        }

        public float NearMissProxy()
        {
            return nearMissProxy;
        }
    }
}

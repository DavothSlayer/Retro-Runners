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
        private int health;
        [SerializeField]
        private int damageToPlayer;        
        public float topSpeed;
        public float targetSpeed;
        public NPCType NPCType;
        [SerializeField]
        private float nearMissProxy;

        public void Damage(int dmg)
        {
            if (health <= 0) return;

            health -= dmg;

            if(health <= 0) HandleDeath();
        }

        [BurstCompile]
        public void Update()
        {
            transform.position += transform.forward * 15f * Time.deltaTime;
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

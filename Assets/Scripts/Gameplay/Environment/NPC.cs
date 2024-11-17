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

        [Header("Wheels")]
        public Transform[] wheelSets;       
        public float wheelDiameter;

        [Header("Core")]
        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;        
        public float topSpeed;
        public float targetSpeed;
        public NPCType NPCType;

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

            foreach (Transform wheels in wheelSets)
            {
                wheels.Rotate(Vector3.right, (15f * Time.deltaTime / wheelDiameter * 3.14f) * 360f, Space.Self);
            }
        }

        [BurstCompile]
        public virtual void HandleDeath()
        {
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
    }
}

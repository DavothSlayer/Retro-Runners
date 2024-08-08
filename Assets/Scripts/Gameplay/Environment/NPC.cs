using Unity.Burst;
using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class NPC : MonoBehaviour, Damageable, NearMissable
    {
        [Header("Core")]
        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;        
        public float topSpeed;
        public float targetSpeed;

        public void Damage(int dmg)
        {
            if (health <= 0) return;

            health -= dmg;

            if(health <= 0) HandleDeath();
        }

        [BurstCompile]
        public virtual void HandleDeath()
        {

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

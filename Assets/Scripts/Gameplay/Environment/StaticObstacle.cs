using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class StaticObstacle : MonoBehaviour, Damageable
    {
        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;

        public int DamageToPlayer()
        {
            return damageToPlayer;
        }

        public int Health()
        {
            return health;
        }

        public virtual void Damage(int dmg)
        {
            health -= dmg;
        }
    }
}
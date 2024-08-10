using System.Runtime.CompilerServices;
using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class GuardRail : MonoBehaviour, Damageable
    {
        public SpawnManager spawnManager;
        public GameObject mainObject;
        public GameObject deadObject;

        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;

        public void ResetRail()
        {
            mainObject.SetActive(true);
            deadObject.SetActive(false);
        }

        public int DamageToPlayer()
        {
            return damageToPlayer;
        }

        public int Health()
        {
            return health;
        }

        public void Damage(int dmg)
        {
            health -= dmg;

            mainObject.SetActive(false);
            deadObject.SetActive(true);
        }
    }
}

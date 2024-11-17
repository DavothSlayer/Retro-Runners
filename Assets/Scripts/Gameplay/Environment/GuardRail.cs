using System.Runtime.CompilerServices;
using Unity.Burst;
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

        [SerializeField]
        private Rigidbody[] rigidBodies;

        public void ResetRail()
        {
            mainObject.SetActive(true);
            deadObject.SetActive(false);

            ResetDead();
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

            ActivateDead(Vector3.up);
        }

        [BurstCompile]
        public void ActivateDead(Vector3 targetVel)
        {
            foreach (Rigidbody t in rigidBodies)
            {
                t.gameObject.SetActive(true);
                t.AddForce(targetVel, ForceMode.VelocityChange);
                t.AddTorque(targetVel / 2f, ForceMode.VelocityChange);
            }
        }

        [BurstCompile]
        public void ResetDead()
        {
            foreach (Rigidbody t in rigidBodies)
            {
                t.gameObject.SetActive(false);
                t.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }
}

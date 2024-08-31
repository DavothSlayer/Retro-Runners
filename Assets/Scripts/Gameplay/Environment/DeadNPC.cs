using Unity.Burst;
using UnityEngine;

namespace RetroCode
{
    public class DeadNPC : MonoBehaviour
    {
        public NPCType NPCType;
        [SerializeField]
        private Rigidbody[] rigidBodies;
        public Rigidbody referenceRigidBody;

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
            foreach(Rigidbody t in rigidBodies)
            {
                t.gameObject.SetActive(false);
                t.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }
}

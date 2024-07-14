using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class DeadNPC : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody[] freeBodies;

        private void OnDisable() => OnReset();

        public void OnDead(Vector3 liveModelVel)
        {
            foreach (Rigidbody rb in freeBodies)
            {
                rb.isKinematic = false;
                rb.velocity = liveModelVel;
                rb.AddExplosionForce(10f, transform.position, 25f);
            }
        }

        public void OnReset()
        {
            foreach(Rigidbody rb in freeBodies)
            {
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
                rb.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }
}

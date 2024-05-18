using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace V3CTOR
{
    public class PhysicsDemo : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody rb;
        [SerializeField]
        private float topSpeed;

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.K))
            {
                rb.AddForce(transform.forward * topSpeed * rb.mass * rb.drag, ForceMode.Force);
            }
        }
    }
}

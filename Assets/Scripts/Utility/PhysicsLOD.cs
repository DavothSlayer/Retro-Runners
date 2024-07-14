using RetroCode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class PhysicsLOD : MonoBehaviour
    {
        [SerializeField]
        private Collider[] colliders;

        private void Update()
        {
            foreach (Collider c in colliders)
                c.enabled = Vector3.Distance(GameManager.playerAutoStatic.transform.position, transform.position) <= 50f;
        }
    }
}

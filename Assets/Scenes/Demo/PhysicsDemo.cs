using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace V3CTOR
{
    public class PhysicsDemo : MonoBehaviour
    {
        public void OnCollisionEnter(Collision collision)
        {
            print("Collided.");
        }

        public void OnCollisionExit(Collision collision)
        {
            print("Exit.");
        }
    }
}

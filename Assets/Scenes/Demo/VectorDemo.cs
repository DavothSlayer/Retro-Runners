using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorDemo : MonoBehaviour
{
    public Transform transform1;
    public Transform transform2;

    public void Update()
    {
        print(Vector3.Dot(transform1.forward, transform2.forward));
    }
}

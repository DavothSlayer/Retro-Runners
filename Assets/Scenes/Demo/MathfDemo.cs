using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathfDemo : MonoBehaviour
{
    public float floatA;
    public float floatB;
    public float floatT;

    private void Update()
    {
        print(Mathf.InverseLerp(floatA, floatB, floatT));
    }
}

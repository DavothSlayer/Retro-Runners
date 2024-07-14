using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class AudioLOD : MonoBehaviour
    {
        [SerializeField]
        private AudioSource[] sources;

        private void Update()
        {
            foreach(AudioSource a in sources)
                a.enabled = Vector3.Distance(GameManager.playerAutoStatic.transform.position, transform.position) <= 100f;
        }
    }
}

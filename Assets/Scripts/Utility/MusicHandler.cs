using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class MusicHandler : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private AudioClip[] music;

        private AudioClip currentTrack;

        private void Awake()
        {
            PlayRandomTrack();
        }

        private void PlayRandomTrack()
        {
            currentTrack = music[Random.Range(0, music.Length)];
            audioSource.clip = currentTrack;

            StartCoroutine(PlayTrackRoutine(currentTrack.length));
        }

        private IEnumerator PlayTrackRoutine(float delay)
        {
            audioSource.Play();

            yield return new WaitForSeconds(delay);

            PlayRandomTrack();
        }
    }
}

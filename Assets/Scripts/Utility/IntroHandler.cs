using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntroHandler : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;

    private void OnEnable() => videoPlayer.loopPointReached += IntroFinished;

    private void OnDisable() => videoPlayer.loopPointReached -= IntroFinished;

    private void IntroFinished(VideoPlayer source)
    {
        SceneManager.LoadScene("MainGame");
    }
}

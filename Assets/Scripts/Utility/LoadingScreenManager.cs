using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RetroCode
{
    public class LoadingScreenManager : MonoBehaviour
    {
        [SerializeField]
        private Image loadingScreen;
        //[SerializeField]
        //private TextMeshProUGUI loadingText;

        private AsyncOperation asyncOP;

        public void EnterSceneMethod(int i)
        {
            //loadingText.color = new Color(1f, 1f, 1f, 0f);

            LeanTween.color(loadingScreen.rectTransform, Color.white, 0.3f);
            //LeanTween.value(gameObject, UpdateTextColor, loadingText.color, new Color(1f, 1f, 1f, 1f), 0.3f);

            EnterSceneRoutine(i);
        }

        private async void EnterSceneRoutine(int i)
        {
            await Task.Delay(500);

            asyncOP = SceneManager.LoadSceneAsync(i);
            asyncOP.allowSceneActivation = false;

            while (!asyncOP.isDone)
            {
                if(asyncOP.progress >= 0.9f)
                {
                    LoadReadyScene();
                }

                await Task.Yield();
            }
        }

        private async void LoadReadyScene()
        {
            LeanTween.color(loadingScreen.rectTransform, Color.black, 0.3f);
            //LeanTween.value(gameObject, UpdateTextColor, loadingText.color, new Color(1f, 1f, 1f, 0f), 0.3f);

            await Task.Delay(500);

            asyncOP.allowSceneActivation = true;
        }

        private void UpdateTextColor(Color color)
        {
            //loadingText.color = color;
        }
    }
}

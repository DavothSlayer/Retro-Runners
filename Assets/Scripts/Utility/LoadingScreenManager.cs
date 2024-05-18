using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RetroCode
{
    public class LoadingScreenManager : MonoBehaviour
    {
        #region References
        [Header("Animation")]
        [SerializeField]
        private Animator canvasAnimator;
        [SerializeField]
        private TextMeshProUGUI loadingStatusText;

        [Header("Screen Content")]
        [SerializeField]
        private Image screenImage;
        [SerializeField]
        private TextMeshProUGUI title;
        [SerializeField]
        private TextMeshProUGUI paragraph;
        [Space]
        [SerializeField]
        private LoadingContentClass[] loadingContent;
        #endregion

        private void Awake()
        {
            SetScreenContent();
        }

        #region Loading Scenes
        private LoadingContentClass contentClass;
        private AsyncOperation asyncOP;
        private bool readyToLoad;

        public void EnterSceneMethod(int i )
        {
            StartCoroutine(EnterSceneRoutine(i));
        }

        private IEnumerator EnterSceneRoutine(int i)
        {
            loadingStatusText.text = "LOADING";
            readyToLoad = false;

            SetLoadingScreenState(1);
            SetScreenContent();

            yield return new WaitForSeconds(0.45f);

            asyncOP = SceneManager.LoadSceneAsync(i);
            asyncOP.allowSceneActivation = false;

            while (!asyncOP.isDone)
            {
                if(asyncOP.progress >= 0.9f)
                {
                    loadingStatusText.text = "TAP  SCREEN  TO  CONTINUE";

                    if (readyToLoad)
                        asyncOP.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        public void SetLoadingScreenState(int i)
        {
            canvasAnimator.SetInteger("Loading Screen State", i);
        }

        public void LoadReadyScene()
        {
            if (asyncOP.progress < 0.9f) return;

            StartCoroutine(LoadReadySceneRoutine());
        }
        private IEnumerator LoadReadySceneRoutine()
        {
            SetLoadingScreenState(0);

            yield return new WaitForSeconds(0.5f);

            readyToLoad = true;
        }

        private void SetScreenContent()
        {
            contentClass = loadingContent[Random.Range(0, loadingContent.Length)];
            screenImage.sprite = contentClass.ScreenGraphic;

            title.text = contentClass.Title;
            paragraph.text = contentClass.Paragraph;
        }
        #endregion
    }

    [System.Serializable]
    public class LoadingContentClass
    {
        public Sprite ScreenGraphic;
        public string Title, Paragraph;
        public bool showContent;
    }
}

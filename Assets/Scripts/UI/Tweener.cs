using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using V3CTOR;
using static UnityEngine.Rendering.DebugUI;

namespace RetroCode
{
    public class Tweener : MonoBehaviour
    {
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        private MainGameHUD hud;
        [SerializeField]
        private RectTransform canvasRect;

        #region Universal Animations
        public void ButtonPressBounce(GameObject gameObject)
        {
            LeanTween.scale(gameObject, new(1.1f, 1.1f, 1f), .1f).setIgnoreTimeScale(true).setEaseInBounce();
            LeanTween.scale(gameObject, Vector3.one, .1f).setDelay(.1f).setIgnoreTimeScale(true);
        }

        public void BounceContinous(GameObject gameObject)
        {
            LeanTween.scale(gameObject, new(1.1f, 1.1f, 1f), .1f).setEaseInBounce();
            LeanTween.scale(gameObject, Vector3.one, .1f).setDelay(.1f).setOnComplete(() => BounceContinousComplete(gameObject));
        }

        private async void BounceContinousComplete(GameObject gameObject)
        {
            await Task.Delay(750);

            BounceContinous(gameObject);
        }

        public void BlinkFadeOut(TextMeshProUGUI text)
        {
            LeanTween.value(gameObject, text.alpha, .1f, .5f)
                .setOnUpdate((float value) =>
                {
                    Color color = text.color;

                    color.a = value;

                    text.color = color;
                })
                .setOnComplete(() => BlinkFadeIn(text));
        }

        public void BlinkFadeIn(TextMeshProUGUI text)
        {
            LeanTween.value(gameObject, text.alpha, 1f, .5f)
                .setOnUpdate((float value) =>
                {
                    Color color = text.color;

                    color.a = value;

                    text.color = color;
                })
                .setOnComplete(() => BlinkFadeOut(text));
        }

        public void ArrowToRight(GameObject gameObject)
        {
            float initX = gameObject.transform.localPosition.x + 10f;
            LeanTween.moveLocalX(gameObject, initX, .35f).setOnComplete(() => ArrowToLeft(gameObject));
        }

        public void ArrowToLeft(GameObject gameObject)
        {
            float initX = gameObject.transform.localPosition.x - 10f;
            LeanTween.moveLocalX(gameObject, initX, .35f).setOnComplete(() => ArrowToRight(gameObject));
        }

        public void FadeInCanvasGroup(CanvasGroup canvasGroup)
        {
            canvasGroup.gameObject.SetActive(true);
            LeanTween.alphaCanvas(canvasGroup, 1f, .15f).setIgnoreTimeScale(true);
        }

        public void FadeOutCanvasGroup(CanvasGroup canvasGroup)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, .25f).setIgnoreTimeScale(true).setOnComplete(() => canvasGroup.gameObject.SetActive(false));
        }

        public void AnimateScoreAdded(TextMeshProUGUI text)
        {
            Color initialColor = text.color;
            initialColor.a = 0f;
            text.color = initialColor;

            // Fade in to alpha 1
            LeanTween.value(gameObject, 0f, 1f, .25f)
                     .setOnUpdate((float alpha) =>
                     {
                         Color newColor = text.color;
                         newColor.a = alpha;
                         text.color = newColor;
                     })
                     .setOnComplete(() =>
                     {
                         // Fade out to alpha 0
                         LeanTween.value(gameObject, 1f, 0f, .5f)
                                  .setOnUpdate((float alpha) =>
                                  {
                                      Color newColor = text.color;
                                      newColor.a = alpha;
                                      text.color = newColor;
                                  });
                     });
        }

        public void SlowerBounce(GameObject gameObject)
        {
            LeanTween.scale(gameObject, new(1.1f, 1.1f, 1f), .25f).setIgnoreTimeScale(true).setEaseInBounce();
            LeanTween.scale(gameObject, Vector3.one, .45f).setDelay(.25f).setIgnoreTimeScale(true);
        }

        public void FadeInNearMissTimer(CanvasGroup canvasGroup)
        {
            LeanTween.alphaCanvas(canvasGroup, 1f, .15f).setIgnoreTimeScale(true);
        }

        public void FadeOutNearMissTimer(CanvasGroup canvasGroup)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, .15f).setIgnoreTimeScale(true);
        }
        #endregion

        #region Game Over Screen Animations
        public void GameOverNormalAnimations()
        {
            hud.gameOverNormalScreen.gameObject.SetActive(true);
            LeanTween.alphaCanvas(hud.gameOverNormalScreen, 1f, 2f).setDelay(2f).setIgnoreTimeScale(true);

            /*LeanTween.value(gameObject, text.alpha, 1f, 1f).setIgnoreTimeScale(true).setDelay(2f)
                .setOnUpdate((float value) =>
                {
                    Color color = text.color;
                    color.a = value;
                    text.color = color;
                });*/

            TweenToScreenPosY(hud.gameOverText.rectTransform, 0.75f, 1f, 4f, ScoreFinalTextAnimation);
        }

        private void ScoreFinalTextAnimation()
        {
            int currentRunScoreRounded = Mathf.RoundToInt(gameManager.currentRunScore);

            LeanTween.value(hud.finalScoreText.alpha, 1f, 1f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    Color color = hud.finalScoreText.color;
                    color.a = value;
                    hud.finalScoreText.color = color;
                });
            LeanTween.value(0f, currentRunScoreRounded, 2f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) => 
                {
                    hud.finalScoreText.text = $"FINAL SCORE {Mathf.RoundToInt(value).ToString("N", EXMET.NumForThou)}";
                });

            TweenToScreenPosY(hud.finalScoreText.rectTransform, 0.635f, 1f, 0.5f, MiscScoreTextAnimations);
        }

        private void MiscScoreTextAnimations()
        {
            // BEST NEARMISS CHAIN //
            LeanTween.value(hud.bestNearMissChainText.alpha, 1f, 1f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    Color color = hud.bestNearMissChainText.color;
                    color.a = value;
                    hud.bestNearMissChainText.color = color;
                });
            LeanTween.value(0f, gameManager.currentRunNMH, 2f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    hud.bestNearMissChainText.text = $"BEST NEAR MISS CHAIN {value.ToString("N", EXMET.NumForThou)}X";
                });
            TweenToScreenPosX(hud.bestNearMissChainText.rectTransform, 0.4f, 1f, 0f, null);

            // COP KILL COUNT //
            LeanTween.value(hud.COPKillCountText.alpha, 1f, 1f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    Color color = hud.COPKillCountText.color;
                    color.a = value;
                    hud.COPKillCountText.color = color;
                });
            LeanTween.value(0f, gameManager.currentRunCOPsDestroyed, 2f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    hud.COPKillCountText.text = $"{value.ToString("N", EXMET.NumForThou)} COP{(gameManager.currentRunCOPsDestroyed != 1 ? "S" : "")} DESTROYED";
                });
            TweenToScreenPosX(hud.COPKillCountText.rectTransform, 0.67f, 1f, 0f, null);

            // NPC KILL COUNT //
            LeanTween.value(hud.NPCKillCountText.alpha, 1f, 1f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    Color color = hud.NPCKillCountText.color;
                    color.a = value;
                    hud.NPCKillCountText.color = color;
                });
            LeanTween.value(0f, gameManager.currentRunNPCsDestroyed, 2f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    hud.NPCKillCountText.text = $"{value.ToString("N", EXMET.NumForThou)} NPC{(gameManager.currentRunNPCsDestroyed != 1 ? "S" : "")} DESTROYED";
                });
            TweenToScreenPosX(hud.NPCKillCountText.rectTransform, 0.4f, 1f, 0f, EarningsTextAnimation);
        }

        private void EarningsTextAnimation()
        {
            LeanTween.value(hud.earningsText.alpha, 1f, 1f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    Color color = hud.earningsText.color;
                    color.a = value;
                    hud.earningsText.color = color;
                });
            LeanTween.value(0f, gameManager.currentRunReward, 3f).setIgnoreTimeScale(true)
                .setOnUpdate((float value) =>
                {
                    hud.earningsText.text = $"R$ {value.ToString("N", EXMET.NumForThou)} EARNED";
                });

            TweenToScreenPosY(hud.earningsText.rectTransform, 0.75f, 1f, 0f, PlayAgainButtonAnimation);
        }

        private void PlayAgainButtonAnimation()
        {
            LeanTween.alphaCanvas(hud.playAgainButtonCanvasGroup, 1f, 1f).setIgnoreTimeScale(true);
        }

        public void ResetGameOverScreen()
        {
            Color color = Color.white;
            color.a = 0f;

            hud.gameOverNormalScreen.gameObject.SetActive(false);
            hud.gameOverNormalScreen.alpha = 0f;

            // RESET THE TEXTS //
            SetToScreenPosY(hud.gameOverText.rectTransform, 0.5f);

            SetToScreenPosY(hud.finalScoreText.rectTransform, 0.5f);
            hud.finalScoreText.color = color;

            SetToScreenPosX(hud.bestNearMissChainText.rectTransform, 0.5f);
            hud.bestNearMissChainText.color = color;

            SetToScreenPosX(hud.COPKillCountText.rectTransform, 0.5f);
            hud.COPKillCountText.color = color;

            SetToScreenPosX(hud.NPCKillCountText.rectTransform, 0.5f);
            hud.NPCKillCountText.color = color;

            SetToScreenPosY(hud.earningsText.rectTransform, 0.5f);
            hud.earningsText.color = color;

            hud.playAgainButtonCanvasGroup.alpha = 0f;
        }

        public void SkipGameOverAnimations()
        {
            Color white = Color.white;

            LeanTween.cancel(GameManager.ZaWarudoID);
            Time.timeScale = 0f;

            hud.gameOverNormalScreen.gameObject.SetActive(true);
            LeanTween.cancel(hud.gameOverNormalScreen.gameObject);
            hud.gameOverNormalScreen.alpha = 1f;

            LeanTween.cancel(hud.gameOverText.gameObject);
            SetToScreenPosY(hud.gameOverText.rectTransform, 0.75f);

            LeanTween.cancel(hud.finalScoreText.gameObject);
            int currentRunScoreRounded = Mathf.RoundToInt(gameManager.currentRunScore);
            hud.finalScoreText.text = $"FINAL SCORE {currentRunScoreRounded.ToString("N", EXMET.NumForThou)}";
            hud.finalScoreText.color = white;
            SetToScreenPosY(hud.finalScoreText.rectTransform, 0.635f);

            LeanTween.cancel(hud.bestNearMissChainText.gameObject);
            hud.bestNearMissChainText.text = $"BEST NEAR MISS CHAIN {gameManager.currentRunNMH}X";
            hud.bestNearMissChainText.color = white;
            SetToScreenPosX(hud.bestNearMissChainText.rectTransform, 0.4f);

            LeanTween.cancel(hud.COPKillCountText.gameObject);
            hud.COPKillCountText.text = $"{gameManager.currentRunCOPsDestroyed} COP{(gameManager.currentRunCOPsDestroyed != 1 ? "S" : "")} DESTROYED";
            hud.COPKillCountText.color = white;
            SetToScreenPosX(hud.COPKillCountText.rectTransform, 0.67f);

            LeanTween.cancel(hud.NPCKillCountText.gameObject);
            hud.NPCKillCountText.text = $"{gameManager.currentRunNPCsDestroyed.ToString("N", EXMET.NumForThou)} NPC{(gameManager.currentRunNPCsDestroyed != 1 ? "S" : "")} DESTROYED";
            hud.NPCKillCountText.color = white;
            SetToScreenPosX(hud.NPCKillCountText.rectTransform, 0.4f);

            LeanTween.cancel(hud.earningsText.gameObject);
            hud.earningsText.text = $"R$ {gameManager.currentRunReward.ToString("N", EXMET.NumForThou)} EARNED";
            hud.earningsText.color = white;
            SetToScreenPosY(hud.earningsText.rectTransform, 0.75f);

            LeanTween.cancel(hud.playAgainButtonCanvasGroup.gameObject);
            hud.playAgainButtonCanvasGroup.alpha = 1f;            
        }
        #endregion

        #region Utils
        private void TweenToScreenPos(RectTransform element, float xPercent, float yPercent, float duration, float delay)
        {
            float targetX = (xPercent - 0.5f) * canvasRect.rect.width;
            float targetY = (yPercent - 0.5f) * canvasRect.rect.height;
            Vector2 targetPosition = new Vector2(targetX, targetY);

            LeanTween.move(element, targetPosition, duration).setIgnoreTimeScale(true).setDelay(delay).setEase(LeanTweenType.easeInOutQuad);
        }

        private void TweenToScreenPosX(RectTransform element, float xPercent, float duration, float delay, Action onComplete)
        {
            float targetX = (xPercent - 0.5f) * canvasRect.rect.width;    
            Vector2 targetPosition = new Vector2(targetX, element.anchoredPosition.y);

            LeanTween.move(element, targetPosition, duration).setIgnoreTimeScale(true).setDelay(delay).setEase(LeanTweenType.easeInOutQuad).setOnComplete(onComplete);
        }

        private void TweenToScreenPosY(RectTransform element, float yPercent, float duration, float delay, Action onComplete)
        {
            float targetY = (yPercent - 0.5f) * canvasRect.rect.height;    
            Vector2 targetPosition = new Vector2(element.anchoredPosition.x, targetY);

            LeanTween.move(element, targetPosition, duration).setIgnoreTimeScale(true).setDelay(delay).setEase(LeanTweenType.easeInOutQuad).setOnComplete(onComplete);
        }

        private void SetToScreenPosX(RectTransform element, float xPercent)
        {
            float targetX = (xPercent - 0.5f) * canvasRect.rect.width;
            Vector2 targetPosition = new Vector2(targetX, element.anchoredPosition.y);

            element.anchoredPosition = targetPosition;
        }

        private void SetToScreenPosY(RectTransform element, float yPercent)
        {
            float targetY = (yPercent - 0.5f) * canvasRect.rect.height;
            Vector2 targetPosition = new Vector2(element.anchoredPosition.x, targetY);

            element.anchoredPosition = targetPosition;
        }
        #endregion
    }
}

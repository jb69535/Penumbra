using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Penumbra.UI
{
    public sealed class StartupLogoSequence : MonoBehaviour
    {
        const string StudioLogoResource = "Logos/Peelson_Studio_Logo";
        const string GameLogoResource = "Logos/Penumbra_Game_Logo_Liminal";
        const float FadeDuration = 0.55f;
        const float StudioHoldDuration = 1.25f;
        const float GameHoldDuration = 1.45f;
        const float GapDuration = 0.25f;
        const float FinalFadeDuration = 0.35f;

        static bool hasPlayed;

        readonly Vector2 studioMaxSize = new(420f, 420f);
        readonly Vector2 gameMaxSize = new(1280f, 560f);

        CanvasGroup canvasGroup;
        Image logoImage;
        RectTransform logoRect;
        float previousTimeScale = 1f;
        bool ownsTimePause;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStartupFlag()
        {
            hasPlayed = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CreateAtStartup()
        {
            if (hasPlayed)
            {
                return;
            }

            hasPlayed = true;
            GameObject host = new("Startup Logo Sequence");
            DontDestroyOnLoad(host);
            host.AddComponent<StartupLogoSequence>();
        }

        void Awake()
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ownsTimePause = true;
            BuildOverlay();
        }

        IEnumerator Start()
        {
            Sprite studioLogo = Resources.Load<Sprite>(StudioLogoResource);
            Sprite gameLogo = Resources.Load<Sprite>(GameLogoResource);

            yield return PlayLogo(studioLogo, studioMaxSize, StudioHoldDuration);
            yield return Wait(GapDuration);
            yield return PlayLogo(gameLogo, gameMaxSize, GameHoldDuration);
            yield return FadeCanvas(1f, 0f, FinalFadeDuration);

            RestoreTimeScale();
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            RestoreTimeScale();
        }

        void BuildOverlay()
        {
            GameObject canvasObject = new("Startup Canvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            canvasGroup = canvasObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            Image background = CreateImage("Black Background", canvasObject.transform);
            background.color = Color.black;
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            logoImage = CreateImage("Logo", canvasObject.transform);
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;
            logoImage.color = new Color(1f, 1f, 1f, 0f);

            logoRect = logoImage.rectTransform;
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = Vector2.zero;
        }

        Image CreateImage(string objectName, Transform parent)
        {
            GameObject imageObject = new(objectName);
            imageObject.transform.SetParent(parent, false);
            return imageObject.AddComponent<Image>();
        }

        IEnumerator PlayLogo(Sprite sprite, Vector2 maxSize, float holdDuration)
        {
            if (sprite == null)
            {
                yield break;
            }

            SetLogo(sprite, maxSize);
            yield return FadeLogo(0f, 1f, FadeDuration);
            yield return Wait(holdDuration);
            yield return FadeLogo(1f, 0f, FadeDuration);
        }

        void SetLogo(Sprite sprite, Vector2 maxSize)
        {
            logoImage.sprite = sprite;
            Vector2 spriteSize = sprite.rect.size;
            float scale = Mathf.Min(maxSize.x / spriteSize.x, maxSize.y / spriteSize.y);
            logoRect.sizeDelta = spriteSize * scale;
        }

        IEnumerator FadeLogo(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetLogoAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetLogoAlpha(to);
        }

        void SetLogoAlpha(float alpha)
        {
            Color color = logoImage.color;
            color.a = alpha;
            logoImage.color = color;
        }

        IEnumerator FadeCanvas(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        IEnumerator Wait(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        void RestoreTimeScale()
        {
            if (!ownsTimePause)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            ownsTimePause = false;
        }
    }
}

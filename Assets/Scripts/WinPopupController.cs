using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPopupController : MonoBehaviour
{
    [Header("Popup")]
    public RectTransform popupBase;
    public RectTransform popupRibbon;
    public Button closeButton;
    public RectTransform congratulationsText;

    [Header("Stars")]
    public RectTransform bigStar;
    public RectTransform smallStarPrefab;
    public RectTransform leftStarOrigin;
    public RectTransform rightStarOrigin;
    public RectTransform starTarget;

    [Header("Timing")]
    public float popupRiseDuration = 0.35f;
    public float textRiseDuration = 0.35f;
    public float starDropDuration = 0.65f;
    public float starSpawnInterval = 0.08f;
    public int smallStarsPerSide = 5;

    [Header("Layout")]
    public Vector2 hiddenPopupOffset = new Vector2(0f, -700f);
    public Vector2 visiblePopupPosition = Vector2.zero;
    public float arcHeight = 120f;
    public float sideArcSpread = 90f;
    public float sideFallDistance = 85f;
    public Vector2 visibleTextPosition = new Vector2(0f, -170f);
    public Vector2 hiddenTextOffset = new Vector2(0f, -260f);

    private Vector2 popupBaseStartSize;

    void Awake()
    {
        EnsureRuntimeHierarchy();

        if (popupBase != null)
        {
            popupBaseStartSize = popupBase.sizeDelta;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ReturnToMainScene);
            closeButton.onClick.AddListener(ReturnToMainScene);
            closeButton.gameObject.SetActive(false);
        }

        if (smallStarPrefab != null)
        {
            smallStarPrefab.gameObject.SetActive(false);
        }
    }

    void EnsureRuntimeHierarchy()
    {
        popupBase = EnsureChildInstance(popupBase, transform, "PopupBase");

        Transform popupParent = popupBase != null ? popupBase : transform;
        popupRibbon = EnsureChildInstance(popupRibbon, popupParent, "PopupRibbon");
        closeButton = EnsureChildInstance(closeButton, popupParent, "CloseButton");
        congratulationsText = EnsureChildInstance(congratulationsText, popupParent, "CongratulationsText");
        bigStar = EnsureChildInstance(bigStar, popupParent, "BigStar");
        smallStarPrefab = EnsureChildInstance(smallStarPrefab, popupParent, "SmallStarTemplate");
        leftStarOrigin = EnsureChildInstance(leftStarOrigin, popupParent, "LeftStarOrigin");
        rightStarOrigin = EnsureChildInstance(rightStarOrigin, popupParent, "RightStarOrigin");
        starTarget = EnsureChildInstance(starTarget, popupParent, "StarTarget");

        ApplyDefaultLayout();
    }

    T EnsureChildInstance<T>(T reference, Transform parent, string instanceName) where T : Component
    {
        if (reference == null || parent == null)
        {
            return reference;
        }

        if (reference.transform.IsChildOf(parent))
        {
            return reference;
        }

        T instance = Instantiate(reference, parent);
        instance.name = instanceName;
        instance.gameObject.SetActive(true);
        return instance;
    }

    void ApplyDefaultLayout()
    {
        if (popupBase != null)
        {
            popupBase.anchorMin = new Vector2(0.5f, 0.5f);
            popupBase.anchorMax = new Vector2(0.5f, 0.5f);
            popupBase.pivot = new Vector2(0.5f, 0.5f);
        }

        if (popupRibbon != null && popupBase != null)
        {
            popupRibbon.anchorMin = new Vector2(0.5f, 1f);
            popupRibbon.anchorMax = new Vector2(0.5f, 1f);
            popupRibbon.pivot = new Vector2(0.5f, 1f);
            popupRibbon.anchoredPosition = Vector2.zero;
            popupRibbon.sizeDelta = new Vector2(popupBase.sizeDelta.x * 1.18f, popupBase.sizeDelta.y * 0.26f);
            popupRibbon.SetAsFirstSibling();
        }

        if (bigStar != null)
        {
            bigStar.anchorMin = new Vector2(0.5f, 0.5f);
            bigStar.anchorMax = new Vector2(0.5f, 0.5f);
            bigStar.pivot = new Vector2(0.5f, 0.5f);
            bigStar.anchoredPosition = new Vector2(0f, 45f);
        }

        if (congratulationsText != null)
        {
            visibleTextPosition = GetCongratulationsTextPosition();
            congratulationsText.anchorMin = new Vector2(0.5f, 0.5f);
            congratulationsText.anchorMax = new Vector2(0.5f, 0.5f);
            congratulationsText.pivot = new Vector2(0.5f, 0.5f);
            congratulationsText.anchoredPosition = visibleTextPosition;
            congratulationsText.sizeDelta = new Vector2(popupBase != null ? popupBase.sizeDelta.x * 0.9f : 320f, 70f);
        }

        if (leftStarOrigin != null)
        {
            leftStarOrigin.anchorMin = new Vector2(0.5f, 0.5f);
            leftStarOrigin.anchorMax = new Vector2(0.5f, 0.5f);
            leftStarOrigin.pivot = new Vector2(0.5f, 0.5f);
            leftStarOrigin.anchoredPosition = new Vector2(-45f, 105f);
        }

        if (rightStarOrigin != null)
        {
            rightStarOrigin.anchorMin = new Vector2(0.5f, 0.5f);
            rightStarOrigin.anchorMax = new Vector2(0.5f, 0.5f);
            rightStarOrigin.pivot = new Vector2(0.5f, 0.5f);
            rightStarOrigin.anchoredPosition = new Vector2(45f, 105f);
        }

        if (starTarget != null)
        {
            starTarget.anchorMin = new Vector2(0.5f, 0.5f);
            starTarget.anchorMax = new Vector2(0.5f, 0.5f);
            starTarget.pivot = new Vector2(0.5f, 0.5f);
            starTarget.anchoredPosition = Vector2.zero;
        }

        if (smallStarPrefab != null)
        {
            smallStarPrefab.anchorMin = new Vector2(0.5f, 0.5f);
            smallStarPrefab.anchorMax = new Vector2(0.5f, 0.5f);
            smallStarPrefab.pivot = new Vector2(0.5f, 0.5f);
            smallStarPrefab.sizeDelta = new Vector2(26f, 26f);
        }

        if (closeButton != null)
        {
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-24f, -24f);
            closeRect.sizeDelta = new Vector2(48f, 48f);
        }
    }

    Vector2 GetCongratulationsTextPosition()
    {
        if (popupBase == null)
        {
            return new Vector2(0f, -170f);
        }

        return new Vector2(0f, -popupBase.sizeDelta.y * 0.34f + 20f);
    }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
        }

        if (popupBase != null)
        {
            popupBase.anchoredPosition = visiblePopupPosition + hiddenPopupOffset;

            if (popupBaseStartSize != Vector2.zero)
            {
                popupBase.sizeDelta = popupBaseStartSize;
            }
        }

        if (bigStar != null)
        {
            bigStar.localScale = Vector3.zero;
        }

        if (congratulationsText != null)
        {
            congratulationsText.anchoredPosition = visibleTextPosition + hiddenTextOffset;
        }

        yield return AnimatePopupRise();
        StartCoroutine(AnimateCongratulationsText());
        yield return AnimateBigStar();
        yield return SpawnSmallStars();

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
        }
    }

    IEnumerator AnimatePopupRise()
    {
        if (popupBase == null)
        {
            yield break;
        }

        Vector2 start = visiblePopupPosition + hiddenPopupOffset;
        Vector2 end = visiblePopupPosition;
        float elapsed = 0f;

        while (elapsed < popupRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popupRiseDuration);
            popupBase.anchoredPosition = Vector2.LerpUnclamped(start, end, EaseOutBack(t));
            yield return null;
        }

        popupBase.anchoredPosition = end;
    }

    IEnumerator AnimateBigStar()
    {
        if (bigStar == null)
        {
            yield break;
        }

        float duration = 0.22f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            bigStar.localScale = Vector3.one * EaseOutBack(t);
            yield return null;
        }

        bigStar.localScale = Vector3.one;
    }

    IEnumerator AnimateCongratulationsText()
    {
        if (congratulationsText == null)
        {
            yield break;
        }

        Vector2 start = visibleTextPosition + hiddenTextOffset;
        Vector2 end = visibleTextPosition;
        float elapsed = 0f;

        while (elapsed < textRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / textRiseDuration);
            congratulationsText.anchoredPosition = Vector2.LerpUnclamped(start, end, EaseOutBack(t));
            yield return null;
        }

        congratulationsText.anchoredPosition = end;
    }

    IEnumerator SpawnSmallStars()
    {
        if (smallStarPrefab == null)
        {
            yield break;
        }

        int count = Mathf.Max(0, smallStarsPerSide);

        for (int i = 0; i < count; i++)
        {
            SpawnSmallStar(leftStarOrigin, -1, i);
            SpawnSmallStar(rightStarOrigin, 1, i);
            yield return new WaitForSeconds(starSpawnInterval);
        }
    }

    void SpawnSmallStar(RectTransform origin, int horizontalDirection, int index)
    {
        if (origin == null || starTarget == null || smallStarPrefab == null)
        {
            return;
        }

        RectTransform star = Instantiate(smallStarPrefab, origin.parent);
        star.gameObject.SetActive(true);
        star.anchoredPosition = origin.anchoredPosition;
        star.localScale = Vector3.one;
        Vector2 endPosition = new Vector2(
            origin.anchoredPosition.x + horizontalDirection * (sideFallDistance + index * 10f),
            starTarget.anchoredPosition.y
        );

        StartCoroutine(AnimateSmallStar(star, origin.anchoredPosition, endPosition, horizontalDirection, index));
    }

    IEnumerator AnimateSmallStar(RectTransform star, Vector2 start, Vector2 end, int horizontalDirection, int index)
    {
        Vector2 control = start + new Vector2(horizontalDirection * (sideArcSpread + index * 8f), arcHeight);

        float elapsed = 0f;

        while (elapsed < starDropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / starDropDuration);
            Vector2 first = Vector2.Lerp(start, control, t);
            Vector2 second = Vector2.Lerp(control, end, t);
            star.anchoredPosition = Vector2.Lerp(first, second, t);
            star.localScale = Vector3.one * Mathf.Lerp(1f, 0.65f, t);
            star.Rotate(0f, 0f, 260f * Time.deltaTime);
            yield return null;
        }

        Destroy(star.gameObject);
    }

    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public void ReturnToMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FailPopupController : MonoBehaviour
{
    [Header("Popup")]
    public RectTransform popupBase;
    public RectTransform popupRibbon;
    public Button closeButton;
    public Button tryAgainButton;

    [Header("Layout")]
    public Vector2 hiddenPopupOffset = new Vector2(0f, -700f);
    public Vector2 visiblePopupPosition = Vector2.zero;
    public Vector2 tryAgainButtonPosition = new Vector2(0f, 40f);

    [Header("Timing")]
    public float popupRiseDuration = 0.35f;

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
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(TryAgain);
            tryAgainButton.onClick.AddListener(TryAgain);
        }
    }

    void EnsureRuntimeHierarchy()
    {
        popupBase = EnsureChildInstance(popupBase, transform, "PopupBase");

        Transform popupParent = popupBase != null ? popupBase : transform;
        popupRibbon = EnsureChildInstance(popupRibbon, popupParent, "PopupRibbon");
        closeButton = EnsureChildInstance(closeButton, popupParent, "CloseButton");
        tryAgainButton = EnsureChildInstance(tryAgainButton, popupParent, "TryAgainButton");

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

        if (closeButton != null)
        {
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-24f, -24f);
            closeRect.sizeDelta = new Vector2(48f, 48f);
        }

        if (tryAgainButton != null)
        {
            tryAgainButtonPosition = GetTryAgainButtonPosition();
            RectTransform tryAgainRect = tryAgainButton.GetComponent<RectTransform>();
            tryAgainRect.anchorMin = new Vector2(0.5f, 0.5f);
            tryAgainRect.anchorMax = new Vector2(0.5f, 0.5f);
            tryAgainRect.pivot = new Vector2(0.5f, 0.5f);
            tryAgainRect.anchoredPosition = tryAgainButtonPosition;
        }
    }

    Vector2 GetTryAgainButtonPosition()
    {
        if (popupBase == null)
        {
            return new Vector2(0f, 40f);
        }

        return new Vector2(0f, 40f);
    }

    public void Play()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.gameObject.SetActive(false);
        }

        if (popupBase != null)
        {
            popupBase.anchoredPosition = visiblePopupPosition + hiddenPopupOffset;

            if (popupBaseStartSize != Vector2.zero)
            {
                popupBase.sizeDelta = popupBaseStartSize;
            }
        }

        yield return AnimatePopupRise();

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.gameObject.SetActive(true);
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

    public void TryAgain()
    {
        SceneManager.LoadScene("LevelScene");
    }
}

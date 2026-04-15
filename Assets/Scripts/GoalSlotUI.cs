using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image checkImage;

    private const float IconSizeRatio = 0.95f;
    private const float CountWidthRatio = 0.65f;
    private const float CountHeightRatio = 0.42f;
    private const float CheckSizeRatio = 0.42f;

    // Initializes the icon and counter.
    public void Setup(Sprite icon, int count)
    {
        EnsureCheckImageInstance();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
        }

        if (checkImage != null)
        {
            checkImage.enabled = false;
            checkImage.preserveAspect = true;
        }

        SetCount(count);
    }

    // Resizes icon, count, and check visuals.
    public void ApplyVisualSize(Vector2 slotSize)
    {
        EnsureCheckImageInstance();

        if (iconImage != null)
        {
            RectTransform iconRect = iconImage.rectTransform;
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = slotSize * IconSizeRatio;
        }

        if (countText != null)
        {
            RectTransform countRect = countText.rectTransform;
            countRect.anchorMin = new Vector2(1f, 0f);
            countRect.anchorMax = new Vector2(1f, 0f);
            countRect.pivot = new Vector2(1f, 0f);
            countRect.anchoredPosition = new Vector2(slotSize.x * 0.12f, -slotSize.y * 0.08f);
            countRect.sizeDelta = new Vector2(slotSize.x * CountWidthRatio, slotSize.y * CountHeightRatio);
            countText.fontSize = Mathf.Max(14f, slotSize.y * 0.42f);
        }

        if (checkImage != null)
        {
            RectTransform checkRect = checkImage.rectTransform;
            checkRect.anchorMin = new Vector2(1f, 0f);
            checkRect.anchorMax = new Vector2(1f, 0f);
            checkRect.pivot = new Vector2(1f, 0f);
            checkRect.anchoredPosition = new Vector2(slotSize.x * 0.12f, -slotSize.y * 0.08f);
            checkRect.sizeDelta = Vector2.one * Mathf.Max(14f, slotSize.y * CheckSizeRatio);
            checkImage.transform.SetAsLastSibling();
        }
    }

    // Instantiates a check image from a prefab reference.
    void EnsureCheckImageInstance()
    {
        if (checkImage == null || checkImage.transform.IsChildOf(transform))
        {
            return;
        }

        Image checkImagePrefab = checkImage;
        checkImage = Instantiate(checkImagePrefab, transform);
        checkImage.name = "CheckImage";
    }

    // Updates the visible remaining count.
    public void SetCount(int count)
    {
        if (countText != null)
        {
            countText.text = Mathf.Max(0, count).ToString();
            countText.enabled = count > 0;
        }

        if (checkImage != null)
        {
            checkImage.enabled = count <= 0;
        }
    }
}

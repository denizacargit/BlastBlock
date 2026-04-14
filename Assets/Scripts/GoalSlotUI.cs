using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    public void Setup(Sprite icon, int count)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        SetCount(count);
    }

    public void SetCount(int count)
    {
        if (countText != null)
        {
            countText.text = Mathf.Max(0, count).ToString();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour
{
    public Image iconImage;
    public Sprite iconSprite;
    public TooltipTrigger tooltipTrigger;

    void Start()
    {
        if(iconSprite != null)
            iconImage.sprite = iconSprite;
    }

    public void SetTooltip(string name, string description)
    {
        tooltipTrigger.SetTooltip(name, description);
    }

    public void SetIcon(Sprite sprite)
    {
        iconSprite = sprite;
        iconImage.sprite = sprite;
    }
}
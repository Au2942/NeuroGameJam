using UnityEngine;
using UnityEngine.UI;

public class WorkerIcon : MonoBehaviour
{
    public WorkerAppearanceData Appearance;
    public UIEventHandler ClickDetector; 
    public RectMask2D BorderMask;
    public Image SelectBorder;
    public Image BG;
    public float PaddingPercentage = 0.1f;
    public Color SelectBorderColor = Color.red;
    public Color UnselectedColor = Color.gray;
    public Color NewWorkerBGColor = Color.blue;
    public Color DefaultBGColor = Color.gray;
    public Image DamageBar;
    public Image CooldownOverlay;

    public void DisplayCooldownOverlay(bool show)
    {
        if(show)
        {
            CooldownOverlay.gameObject.SetActive(true);
        }
        else
        {
            CooldownOverlay.gameObject.SetActive(false);
        }
    }

    public void SetCooldownOverlay(float value)
    {
        CooldownOverlay.fillAmount = value;
    }

    public void SetDamageBar(float value)
    {
        DamageBar.fillAmount = value;
    }

    public void ShowNewWorkerBG(bool show)
    {
        if(show)
        {
            BG.color = NewWorkerBGColor;
        }
        else
        {
            BG.color = DefaultBGColor;
        }
    }

    public void ShowSelectBorder(bool show)
    {
        if(show)
        {
            SelectBorder.color = SelectBorderColor;
            float paddingX = SelectBorder.rectTransform.rect.width * PaddingPercentage/2;
            float paddingY = SelectBorder.rectTransform.rect.height * PaddingPercentage/2;
            BorderMask.padding = new Vector4(
                paddingX, 
                paddingY, 
                paddingX, 
                paddingY
            );
        }
        else
        {
            SelectBorder.color = UnselectedColor;
            BorderMask.padding = new Vector4(0, 0, 0, 0);
        }
    }

}
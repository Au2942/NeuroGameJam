
using UnityEngine;
using UnityEngine.UI;

public class WorkerAppearance : MonoBehaviour
{
    [SerializeField] public WorkerAppearanceData WorkerAppearanceData = new WorkerAppearanceData();

    public void SetApperance(WorkerAppearanceData WAD)
    {
        SetSprite(WAD.Body.sprite, WAD.Face.sprite, WAD.PropellerArms.sprite, WAD.WorkingArms.sprite);
        SetColor(WAD.Body.color, WAD.Screen.color, WAD.PropellerArms.color, WAD.WorkingArms.color);
    }

    public void SetSprite(Sprite body, Sprite Face, Sprite PropellerArms, Sprite WorkingArms)
    {
        if(body != null)
            WorkerAppearanceData.Body.sprite = body;
        if(Face != null)
            WorkerAppearanceData.Face.sprite = Face;
        if(PropellerArms != null)
            WorkerAppearanceData.PropellerArms.sprite = PropellerArms;
        if(WorkingArms != null)    
            WorkerAppearanceData.WorkingArms.sprite = WorkingArms;
    }

    public void SetColor(Color body, Color screen, Color propellerArms, Color workingArms)
    {
        if(body != null)
            WorkerAppearanceData.Body.color = body;
        if(screen != null)
            WorkerAppearanceData.Screen.color = screen;
        if(propellerArms != null)
            WorkerAppearanceData.PropellerArms.color = propellerArms;
        if(workingArms != null)
            WorkerAppearanceData.WorkingArms.color = workingArms;
    }
     
}
[System.Serializable]
public struct WorkerAppearanceData
{
    public Image Body;
    public Image Screen;
    public Image Face;
    public Image PropellerArms;
    public Image Propeller;
    public Image WorkingArms;

    public WorkerAppearanceData(Image body, Image screen, Image face, Image propellerArms, Image propeller, Image workingArms)
    {
        Body = body;
        Screen = screen;
        Face = face;
        PropellerArms = propellerArms;
        Propeller = propeller;
        WorkingArms = workingArms;
    }
}
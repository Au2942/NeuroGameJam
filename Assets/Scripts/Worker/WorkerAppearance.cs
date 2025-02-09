
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerAppearance : MonoBehaviour
{
    [SerializeField] public WorkerAppearanceData WorkerAppearanceData = new WorkerAppearanceData();
    [SerializeField] public RectTransform AppearanceRect;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material tpMaterial;
    private Material tempTPMaterial;
    private Queue<Material> originalMaterials = new Queue<Material>();
    private bool isTeleporting = false;
    private bool outlineVisible = false;

    void Start()
    {
        if(AppearanceRect == null)
            AppearanceRect = GetComponent<RectTransform>();

        if(tpMaterial != null)
            tempTPMaterial = new Material(tpMaterial);
        gameObject.SetActive(false);
    }

    public void ShowOutline(bool show)
    {
        if(outlineMaterial == null || isTeleporting)
        {
            return;
        }
        outlineVisible = show;
        if(outlineVisible)
        {
            WorkerAppearanceData.Body.material = outlineMaterial;
            WorkerAppearanceData.PropellerArms.material = outlineMaterial;
            WorkerAppearanceData.WorkingArms.material = outlineMaterial;
        }

    }

    public IEnumerator PlayTeleportEffect(float duration, bool reverse = false)
    {
        if(tempTPMaterial == null)
        {
            yield break;
        }
        ShowOutline(false);
        isTeleporting = true;
        foreach(Image image in WorkerAppearanceData)
        {
            originalMaterials.Enqueue(image.material);
            image.material = tempTPMaterial;
        }
        Image body = WorkerAppearanceData.Body;
        body.materialForRendering.SetFloat("_Seed", Random.Range(0, 1000));
        float elapsedTime = 0f;
        while(elapsedTime < duration)
        {
            float progress = reverse ? 1 - elapsedTime / duration : elapsedTime / duration;
            body.materialForRendering.SetFloat("_Progress", progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        body.materialForRendering.SetFloat("_Progress", reverse ? 0 : 1);
        
        foreach(Image image in WorkerAppearanceData)
        {
            image.material = originalMaterials.Dequeue();
        }
        isTeleporting = false;
        ShowOutline(outlineVisible);
    }

    public void SetApperance(WorkerAppearanceData WAD)
    {
        SetSprite( WAD.PropellerArms.sprite, WAD.WorkingArms.sprite,WAD.Body.sprite, WAD.Face.sprite);
        SetColor( WAD.PropellerArms.color, WAD.WorkingArms.color,WAD.Body.color, WAD.Screen.color);
    }

    public void SetSprite( Sprite PropellerArms, Sprite WorkingArms,Sprite body, Sprite Face)
    {
        if(PropellerArms != null)
            WorkerAppearanceData.PropellerArms.sprite = PropellerArms;
        if(WorkingArms != null)    
            WorkerAppearanceData.WorkingArms.sprite = WorkingArms;
        if(body != null)
            WorkerAppearanceData.Body.sprite = body;
        if(Face != null)
            WorkerAppearanceData.Face.sprite = Face;
    }

    public void SetColor( Color propellerArms, Color workingArms,Color body, Color screen)
    {
        if(propellerArms != null)
            WorkerAppearanceData.PropellerArms.color = propellerArms;
        if(workingArms != null)
            WorkerAppearanceData.WorkingArms.color = workingArms;
        if(body != null)
            WorkerAppearanceData.Body.color = body;
        if(screen != null)
            WorkerAppearanceData.Screen.color = screen;
    }

    void OnDestroy()
    {
        if(tempTPMaterial != null)
        {
            DestroyImmediate(tempTPMaterial);
        }
    }
     
}
[System.Serializable]
public struct WorkerAppearanceData
{
    public Image PropellerArms;
    public Image Propeller;
    public Image WorkingArms;
    public Image Body;
    public Image Screen;
    public Image Face;
    
    public WorkerAppearanceData( Image propellerArms, Image propeller, Image workingArms,Image body, Image screen, Image face)
    {
        PropellerArms = propellerArms;
        Propeller = propeller;
        WorkingArms = workingArms;
        Screen = screen;
        Face = face;
        Body = body;
    }

    public void CopyData(WorkerAppearanceData appearanceData)
    {
        PropellerArms.sprite = appearanceData.PropellerArms.sprite;
        PropellerArms.color = appearanceData.PropellerArms.color;
        Propeller.sprite = appearanceData.Propeller.sprite;
        Propeller.color = appearanceData.Propeller.color;
        WorkingArms.sprite = appearanceData.WorkingArms.sprite;
        WorkingArms.color = appearanceData.WorkingArms.color;
        Body.sprite = appearanceData.Body.sprite;
        Body.color = appearanceData.Body.color;
        Screen.sprite = appearanceData.Screen.sprite;
        Screen.color = appearanceData.Screen.color;
        Face.sprite = appearanceData.Face.sprite;
        Face.color = appearanceData.Face.color;
    }

    public IEnumerator GetEnumerator()
    {
        yield return PropellerArms;
        yield return Propeller;
        yield return WorkingArms;
        yield return Body;
        yield return Screen;
        yield return Face;
    }
}
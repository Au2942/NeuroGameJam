
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerAppearance : MonoBehaviour
{
    [SerializeField] public WorkerAppearanceData WorkerAppearanceData = new WorkerAppearanceData();
    [SerializeField] public RectTransform AppearanceRect;
    [SerializeField] private Material tpMaterial;
    private Material tempMaterial;
    private Queue<Material> originalMaterials = new Queue<Material>();

    void Start()
    {
        if(AppearanceRect == null)
            AppearanceRect = GetComponent<RectTransform>();

        if(tpMaterial != null)
            tempMaterial = new Material(tpMaterial);
        gameObject.SetActive(false);
    }

    public IEnumerator PlayTeleportEffect(float duration, bool reverse = false)
    {
        if(tempMaterial == null)
        {
            yield break;
        }

        foreach(Image image in WorkerAppearanceData)
        {
            originalMaterials.Enqueue(image.material);
            image.material = tempMaterial;
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
    }

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

    void OnDestroy()
    {
        if(tempMaterial != null)
        {
            DestroyImmediate(tempMaterial);
        }
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

    public void CopyData(WorkerAppearanceData appearanceData)
    {
        Body.sprite = appearanceData.Body.sprite;
        Body.color = appearanceData.Body.color;
        Screen.sprite = appearanceData.Screen.sprite;
        Screen.color = appearanceData.Screen.color;
        Face.sprite = appearanceData.Face.sprite;
        Face.color = appearanceData.Face.color;
        PropellerArms.sprite = appearanceData.PropellerArms.sprite;
        PropellerArms.color = appearanceData.PropellerArms.color;
        Propeller.sprite = appearanceData.Propeller.sprite;
        Propeller.color = appearanceData.Propeller.color;
        WorkingArms.sprite = appearanceData.WorkingArms.sprite;
        WorkingArms.color = appearanceData.WorkingArms.color;
    }

    public IEnumerator GetEnumerator()
    {
        yield return Body;
        yield return Screen;
        yield return Face;
        yield return PropellerArms;
        yield return Propeller;
        yield return WorkingArms;
    }
}
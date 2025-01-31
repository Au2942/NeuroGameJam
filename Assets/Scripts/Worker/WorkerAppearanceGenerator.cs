using UnityEngine;

public class WorkerAppearanceGenerator : MonoBehaviour
{
    [SerializeField] public Sprite[] Body;
    [SerializeField] public Sprite[] Screen;
    [SerializeField] public Sprite[] Face;
    [SerializeField] public Sprite[] PropellerArms;
    [SerializeField] public Sprite[] Propeller;
    [SerializeField] public Sprite[] WorkingArms;
    [SerializeField] public Vector3[] BodyColor = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 1, 1) };
    [SerializeField] public Vector3[] ScreenColor = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 1, 1) };
    [SerializeField] public Vector3[] PropellerArmsColor = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 1, 1) };
    [SerializeField] public Vector3[] WorkingArmsColor = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 1, 1) };


    public void GenerateAppearance(WorkerAppearance WA)
    {
        //body screen
        int headIndex = Random.Range(0, Body.Length);

        //face
        int faceIndex = Random.Range(0, Face.Length);

        //propeller
        int propellerIndex = Random.Range(0, PropellerArms.Length);

        //working arms
        int workingArmsIndex = Random.Range(0, WorkingArms.Length);

        WA.SetSprite(Body[headIndex], Face[faceIndex], PropellerArms[propellerIndex], WorkingArms[workingArmsIndex]);
        
        WA.SetColor(GetRandomColorHSV(BodyColor), GetRandomColorHSV(ScreenColor), GetRandomColorHSV(PropellerArmsColor), GetRandomColorHSV(WorkingArmsColor));
    }

    private Color GetRandomColorHSV(Vector3[] colorRange)
    {
        return Color.HSVToRGB(Random.Range(colorRange[0].x, colorRange[1].x), Random.Range(colorRange[0].y, colorRange[1].y), Random.Range(colorRange[0].z, colorRange[1].z));
    }
    
}
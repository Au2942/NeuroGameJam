using UnityEngine;

[CreateAssetMenu(fileName = "StreamCardSO", menuName = "Scriptable Objects/StreamCardSO")]
public class StreamCardSO : ScriptableObject
{
    public StreamSO stream;
    public Sprite icon;
    public string streamDesc;
    
}

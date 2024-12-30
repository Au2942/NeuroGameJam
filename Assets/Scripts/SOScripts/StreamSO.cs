using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "StreamSO", menuName = "Scriptable Objects/StreamSO")]
public class StreamSO : ScriptableObject
{
    public string id;
    public VideoClip clip;
    public GameObject memory;
    
}

using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "StreamSO", menuName = "Scriptable Objects/StreamSO")]
public class StreamSO : ScriptableObject
{
    public VideoClip clip;
    public GameObject memory;
    
}

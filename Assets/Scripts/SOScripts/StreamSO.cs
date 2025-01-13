using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "StreamSO", menuName = "Scriptable Objects/StreamSO")]
public class StreamSO : ScriptableObject
{
    public string streamName;
    public StreamEntity stream;
    public MemoryEntity memory;
    
}

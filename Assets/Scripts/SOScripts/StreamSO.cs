using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "StreamSO", menuName = "Scriptable Objects/StreamSO")]
public class StreamSO : ScriptableObject
{
    public string streamName;
    public StreamEntity stream;
    public MemoryEntity memory;

    public float hypePotential = 0.2f;

    public float hypePeakDuration = 10f;

    public float impactHype = 0f;
    public float hypeGain = 0.1f;
    public float hypeDrop = 0.1f;
    
}

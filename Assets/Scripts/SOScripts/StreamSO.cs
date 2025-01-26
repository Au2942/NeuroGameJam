using UnityEngine;

[CreateAssetMenu(fileName = "StreamSO", menuName = "Scriptable Objects/StreamSO")]
public class StreamSO : ScriptableObject
{
    public string streamName;
    public StreamEntity stream;
    public MemoryEntity memory;

    public float interestsPotential = 0.2f;

    public float interestsPeakDuration = 10f;

    public float impactInterests = 0f;
    public float interestsGain = 0.1f;
    public float interestsDrop = 0.1f;
    public float viewsBonus = 0f;
    public float subsBonus = 0f;
    
}

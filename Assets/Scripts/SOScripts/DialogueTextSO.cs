using UnityEngine;

[CreateAssetMenu(fileName = "DialogueInfoSO", menuName = "Scriptable Objects/DialogueInfoSO")]
public class DialogueInfoSO : ScriptableObject
{
    public string speakerName;
    [TextArea(3, 10)]
    public string[] dialogueText;
    public float speakSpeed = 0.04f;

    [Header("Audio")]
    public AudioClip[] audioClips;
    [Range(-3,3)]
    public float MaxPitch = 1;
    [Range(-3,3)]
    public float MinPitch = 0;
    public int frequency = 2;
}

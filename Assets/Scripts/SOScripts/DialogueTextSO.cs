using UnityEngine;

[CreateAssetMenu(fileName = "DialogueTextSO", menuName = "Scriptable Objects/DialogueTextSO")]
public class DialogueTextSO : ScriptableObject
{
    public string speakerName;
    [TextArea(3, 10)]
    public string[] dialogueText;

    [Header("Audio")]
    public AudioClip[] audio;
    public int MaxPitch = 1;
    public int MinPitch = 0;
    public int frequency = 2;
}

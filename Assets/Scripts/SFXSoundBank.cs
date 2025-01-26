using UnityEngine;

public class SFXSoundBank : MonoBehaviour
{
    public static SFXSoundBank Instance;
    [SerializeField] private AudioClip invalidActionSFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayInvalidActionSFX()
    {
        if (invalidActionSFX != null)
        SFXManager.Instance.PlaySoundFX(invalidActionSFX);
    }
}
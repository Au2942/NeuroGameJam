using UnityEngine;

public class StreamActionsButton : MonoBehaviour
{
    [SerializeField] private AudioClip invalidActionSound;

    public void PlayInvalidActionSound()
    {
        SFXManager.Instance.PlaySoundFX(invalidActionSound);
    }

    public void StreamSelectorAction()
    {
        if(!PlayerManager.Instance.TryOpenStreamSelector())
        {
            PlayInvalidActionSound();
        }
    }
    public void ResetAction()
    {
        if(!PlayerManager.Instance.TryReset())
        {
            PlayInvalidActionSound();
        }
    }

    public void SleepAction()
    {
        if(!PlayerManager.Instance.TrySleep())
        {
            PlayInvalidActionSound();
        }
    }

    public void SettingsAction()
    {

    }

}
using UnityEngine;

public class StreamActionsButton : MonoBehaviour
{
    public void StreamSelectorAction()
    {
        if(!PlayerManager.Instance.TryOpenStreamSelector())
        {
            SFXSoundBank.Instance.PlayInvalidActionSFX();
        }
    }
    public void ResetAction()
    {
        if(!PlayerManager.Instance.TryReset())
        {
            SFXSoundBank.Instance.PlayInvalidActionSFX();
        }
    }

    public void SleepAction()
    {
        if(!PlayerManager.Instance.TryOpenSleepSettingsScreen())
        {
            SFXSoundBank.Instance.PlayInvalidActionSFX();
        }
    }

    public void SettingsAction()
    {

    }

}
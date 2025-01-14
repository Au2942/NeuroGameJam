using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;
    [SerializeField] private DialogueManager endGameDialogueManager;
    [SerializeField] private DialogueInfoSO[] endGameDialogues;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private AudioClip[] endGameSFXs;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator EndGame(int endingIndex)
    {
        GameManager.Instance.StopStream();

        endGameDialogueManager.gameObject.SetActive(true);
        if(endGameSFXs[endingIndex] != null)
        {
            SFXManager.Instance.PlaySoundFX(endGameSFXs[endingIndex], transform);
            yield return new WaitForSeconds(endGameSFXs[endingIndex].length);
        }
        
        endGameDialogueManager.PlayDialogue(endGameDialogues[endingIndex]);
        endScoreText.text = "You've got " + PlayerManager.Instance.CurrentViewers + " viewers!";
        while(endGameDialogueManager.IsTyping)
        {
            yield return null;
        }
        while(true)
        {
            if(InputManager.Instance.Submit.triggered || InputManager.Instance.Cancel.triggered || InputManager.Instance.Click.triggered) 
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield break;
            }
            yield return null;
        }
    }
}
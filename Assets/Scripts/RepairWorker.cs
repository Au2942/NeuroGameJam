using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RepairWorker : MonoBehaviour
{
    [SerializeField] public string Identifier;
    [SerializeField] public Image WorkerIcon;
    [SerializeField] public TextMeshProUGUI NameText;
    [SerializeField] public UIEventHandler ClickHandler; //can be put into its own class later maybe?
    [SerializeField] public Image CooldownIcon;

    [SerializeField] public float RepairSpeed = 5f;
    [SerializeField] public float RepairAmount = 30f;
    [SerializeField] public float Cooldown = 5f;
    [SerializeField] public bool IsAvailable = true;

    public event System.Action<RepairWorker> OnSelected; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClickHandler.OnLeftClickEvent += (eventData) => Selected();
    }

    public void Selected()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair || !IsAvailable) return;
        PlayerManager.Instance.SetState(PlayerManager.PlayerState.repair);
        SetAvailability(false);
        OnSelected?.Invoke(this);
    }

    public void SetAvailability(bool availability)
    {
        IsAvailable = availability;
        if(IsAvailable)
        {
            CooldownIcon.gameObject.SetActive(false);
        }
        else
        {
            CooldownIcon.gameObject.SetActive(true);
        }
    }
  

    public void FinishWork()
    {
        StartCoroutine(StartCooldown());
    }

    public IEnumerator StartCooldown()
    {
        float elapsedTime = 0f;;
        while(elapsedTime < Cooldown)
        {
            CooldownIcon.fillAmount = 1-(elapsedTime / Cooldown);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetAvailability(true);
    }
    

    
    void OnDestroy()
    {
        ClickHandler.OnLeftClickEvent -= (eventData) => Selected();
    }
}

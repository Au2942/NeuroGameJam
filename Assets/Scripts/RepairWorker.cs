using UnityEngine;
using TMPro;

public class RepairWorker : MonoBehaviour
{
    [SerializeField] public string identifier;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] public UIEventHandler clickHandler; //can be put into its own class later maybe?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clickHandler.OnLeftClickEvent += (eventData) => EnterRepairState();
    }

    void EnterRepairState()
    {
        if(PlayerManager.Instance.state == PlayerManager.PlayerState.repair) return;
        PlayerManager.Instance.SetState(PlayerManager.PlayerState.repair);
    }
    
    void OnDestroy()
    {
        clickHandler.OnLeftClickEvent -= (eventData) => EnterRepairState();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class SpinMemory : MemoryEntity
{
    [SerializeField] public float currentSpeed = 0f;
    [SerializeField] public float maxAccerelation = 4f;
    [SerializeField] public float SpinSpeedIncrement = 0.1f;
    [SerializeField] public float SpinSpeedDecrement = 1f;
    [SerializeField] public float startDealingDamageSpeed = 100f;
    [SerializeField] public float delayBetweenDamage = 4f;
    [SerializeField] public UIEventHandler[] holdDetectors;
    private float currentAcceleration = 0f;
    private float currentDeceleration = 0f; 
    private float damageTimer = 0f;
    private bool isHolding = false;

    protected override void Start()
    {
        base.Start();
        foreach(UIEventHandler holdDetector in holdDetectors)
        {
            holdDetector.OnPointerDownEvent += (eventData) => isHolding = true;
            holdDetector.OnPointerUpEvent += (eventData) => isHolding = false;
            holdDetector.OnPointerExitEvent += (eventData) => isHolding = false;
        }

    }



    void UpdateSpinSpeed()
    {
        currentAcceleration = 1f + (1f - Health / MaxHealth) * (maxAccerelation - 1f);
    }

    public override void EnterCorruptState()
    {
        base.EnterCorruptState();
        currentSpeed = 100;
    }

    protected override void SharedBehavior()
    {
        base.SharedBehavior();

        UpdateSpinSpeed();

        if (isHolding)
        {
            Debug.Log("Holding");
            currentDeceleration += SpinSpeedDecrement * Time.deltaTime;
            currentSpeed = Mathf.Max(0, currentSpeed - currentDeceleration * Time.deltaTime);
        }
        else
        {
            currentDeceleration = 0f;
            currentSpeed += currentAcceleration * SpinSpeedIncrement * Time.deltaTime;
        }

            Body.transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);

    }


    protected override void CorruptBehavior()
    {
        base.CorruptBehavior();

        if (damageTimer >= delayBetweenDamage)
        {
            PlayerManager.Instance.TakeDamage(1);
            damageTimer = 0f;
        }
        else damageTimer += Time.deltaTime;

        if(currentSpeed <= 50)
        {
            ExitCorruptState();
        }

    }

    public override void ExitCorruptState()
    {
        base.ExitCorruptState();
        currentSpeed = 0f;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach(UIEventHandler holdDetector in holdDetectors)
        {
            holdDetector.OnPointerDownEvent -= (eventData) => isHolding = true;
            holdDetector.OnPointerUpEvent -= (eventData) => isHolding = false;
            holdDetector.OnPointerExitEvent -= (eventData) => isHolding = false;
        }
    }


}
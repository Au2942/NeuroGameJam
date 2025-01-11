using UnityEngine;
using UnityEngine.EventSystems;

public class SpinNeuro : MemoryEntity
{
    [SerializeField] public float currentSpeed = 1f;
    [SerializeField] public float maxAccerelation = 4f;
    [SerializeField] public float SpinSpeedIncrement = 0.1f;
    [SerializeField] public float SpinSpeedDecrement = 0.5f;
    [SerializeField] public float startDealingDamageSpeed = 100f;
    [SerializeField] public float delayBetweenDamage = 4f;
    [SerializeField] public DetectUIHold[] holdDetectors;
    private float currentAcceleration = 0f;
    private float currentDeceleration = 0f; 
    private float damageTimer = 0f;
    private bool isHolding = false;

    protected override void Start()
    {
        base.Start();
        foreach(DetectUIHold holdDetector in holdDetectors)
        {
            holdDetector.OnBeginHoldEvent += (eventData) => isHolding = true;
            holdDetector.OnEndHoldEvent += (eventData) => isHolding = false;
        }

    }

    protected override void Update()
    {
        base.Update();
        if(!GameManager.Instance.isStreaming) return;
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

        // Rotate the object
        foreach(GameObject appearance in appearancePhases)
        {
            appearance.transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);
        }
    }


    void UpdateSpinSpeed()
    {
        currentAcceleration = 1f + (1f - Integrity / (float)MaxIntegrity) * (maxAccerelation - 1f);
    }


    protected override void PhaseOneBehaviour()
    {

    }

    protected override void PhaseTwoBehaviour()
    {
        
        if (currentSpeed >= startDealingDamageSpeed)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= delayBetweenDamage)
            {
                PlayerManager.Instance.TakeDamage(1);
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    protected override void PhaseThreeBehaviour()
    {
        if (currentSpeed >= startDealingDamageSpeed)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= delayBetweenDamage/2f)
            {
                PlayerManager.Instance.TakeDamage(1);
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }


}
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] public int Score = 3000; //ccv 
    [SerializeField] public int MaxHealth = 100; //stability
    [SerializeField] public int Health = 100; //stability
    [SerializeField] public float StreamTime = 60f;
    [SerializeField] public float StreamTimeScale = 1f;
    [SerializeField] public float StreamTimeIncrease = 10f;
    [SerializeField] private float addScorePercentage = 1;
    [SerializeField] private float minAddScoreInterval = 5f; 
    [SerializeField] private float maxAddScoreInterval = 20f; 
    [SerializeField] public float Hype = 0;

    private float nextScoreTime = 0f;
    private float nextScoreTimer = 0f;
    private float currentStreamElapsedTime = 0f;

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

    public void ProgressStream()
    {

        currentStreamElapsedTime += Time.deltaTime;
        if (currentStreamElapsedTime >= StreamTime)
        {
            // if(Score >= 100000)
            // {
            //     StartCoroutine(EndingManager.Instance.EndGame(2));
            // }
            // else if(Score <= 0)
            // {
            //     StartCoroutine(EndingManager.Instance.EndGame(1));
            // }
            currentStreamElapsedTime = 0f;
        }
        else
        {
            nextScoreTimer += Time.deltaTime;
            if (nextScoreTimer >= nextScoreTime)
            {
                nextScoreTimer = 0f;
                AddScoreByPercentage(addScorePercentage, 10);
                nextScoreTime = Random.Range(minAddScoreInterval, maxAddScoreInterval);
            }
        }
    }

    public void IncreaseStreamTime()
    {
        StreamTime += StreamTimeIncrease;
        currentStreamElapsedTime = 0f;
    }

    public void AddScore(int value)
    {
        Score += value;
    }

    public void AddScoreByPercentage(float percentage, float errorPercentage)
    {   
        float valueOfPercentage = Score * percentage / 100;

        float errorMargin = valueOfPercentage * errorPercentage / 100;
        float randomValue = Random.Range(valueOfPercentage - errorMargin, valueOfPercentage + errorMargin);
        Score += Mathf.RoundToInt(randomValue);
        if(Score < 0)
        {
            //to-do game over
            Score = 0;
        }   
    }

    public void TakeDamage(int value)
    {
        Health -= value;
        if(Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        if(Health < 0)
        {
            //to-do game over
            Health = 0;
            StartCoroutine(EndingManager.Instance.EndGame(0));
        }
    }


}

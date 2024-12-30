using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public int Score {get; set; }= 3000; //ccv 
    public int Health {get; set; }= 100; //stability
    public float Buff {get; set; }= 0;
    public float Debuff {get; set; }= 0;

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

    public void AddScore(int value)
    {
        Score += value;
    }

    public void AddScoreByPercentage(float percentage, float errorPercentage)
    {   
        float valueOfPercentage = Score * percentage / 100;

        float errorMargin = valueOfPercentage * errorPercentage / 100;
        float randomValue = Random.Range(valueOfPercentage - errorMargin, valueOfPercentage + errorMargin);
        randomValue = randomValue * (1+Buff-Debuff);
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
        if(Health > 100)
        {
            Health = 100;
        }
        if(Health < 0)
        {
            //to-do game over
            Health = 0;
        }
    }


}

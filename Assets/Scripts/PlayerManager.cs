using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public int Score {get; set; }= 3000; //ccv 
    public int Health {get; set; }= 10000; //stability
    public int Streak {get; set; }= 0;
    public int BadStreak {get; set; }= 0;

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

    public void AddUncertainScore(int baseValue, int errorPercentage)
    {
        int errorMargin = (int)(baseValue * errorPercentage / 100);
        int randomValue = Random.Range(baseValue - errorMargin, baseValue + errorMargin);
        if(Streak > 0)
        {
            randomValue = Mathf.RoundToInt(randomValue*(1+Streak/10));
        }
        if(BadStreak > 0)
        {
            randomValue = Mathf.RoundToInt(randomValue*-(1+BadStreak/10));
        }
        Score += randomValue;
        if(Score < 0)
        {
            Score = 0;
        }   
    }


    public void TakeDamage(int value)
    {
        Health -= value;
    }


}

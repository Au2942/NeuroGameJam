using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static TimescaleManager Instance;
    public float defaultTimescale = 1f;
    [SerializeField] public float displayTimeMultiplier = 60f;


    private void Awake()
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

    public void SetTimescale(float timescale)
    {
        Time.timeScale = timescale;
    }

    public void ResetTimescale()
    {
        Time.timeScale = defaultTimescale;
    }
    
}
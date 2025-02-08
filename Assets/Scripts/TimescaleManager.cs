using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static TimescaleManager Instance;
    public float defaultTimescale = 1f;
    [SerializeField] public float displayTimeMultiplier = 60f;

    private float actualTimeScale = 1f;


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

    public void PauseTimeScale()
    {
        Time.timeScale = 0f;
    }

    public void UnpauseTimeScale()
    {
        Time.timeScale = actualTimeScale;
    }

    public void SetTimescale(float timescale)
    {
        Time.timeScale = timescale;
        actualTimeScale = timescale;
    }

    public void ResetTimescale()
    {
        Time.timeScale = defaultTimescale;
        SetTimescale(defaultTimescale);
    }
    
}
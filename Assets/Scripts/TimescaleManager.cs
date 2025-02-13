using UnityEngine;
using System.Text;
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

    public string FormatTimeString(float timeInSeconds, bool scale = true)
    {   
        float scaledTime = scale ? timeInSeconds * displayTimeMultiplier : timeInSeconds;
        StringBuilder sb = new StringBuilder();
        int hours = Mathf.FloorToInt(scaledTime / 3600F);
        int minutes = Mathf.FloorToInt((scaledTime - hours * 3600) / 60F);
        int seconds = Mathf.FloorToInt(scaledTime - hours * 3600 - minutes * 60);
        sb.Clear();
        sb.Append(hours.ToString("00")).Append(":").Append(minutes.ToString("00")).Append(":").Append(seconds.ToString("00"));
        return sb.ToString(); 
    }
    
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerViewersHandler : MonoBehaviour
{
    [SerializeField] public int BaselineViewers = 5000;
    [SerializeField] private float baselineChangePercentage = 1f;
    [SerializeField] private float baselineUpdateInterval = 5f;
    [SerializeField] private int expectedViewers = 0;
    [SerializeField] private float viewersAdjustmentPercentage = 5f;
    [SerializeField] private float minUpdateViewersInterval = 3f; 
    [SerializeField] private float maxUpdateViewersInterval = 6f; 
    [SerializeField] private float viewersNoisePercentage = 0.1f;
    [SerializeField] private float minUpdateNoiseInterval = 1f;
    [SerializeField] private float maxUpdateNoiseInterval = 5f;






    public IEnumerator SimulateViewersMovement()
    {
        while (true)
        {
            while(!GameManager.Instance.isStreaming)
            {
                yield return null;
            }

            float interval = Random.Range(minUpdateViewersInterval, maxUpdateViewersInterval);

            // Calculate the percentage adjustment toward the expected viewers
            float adjustment = (expectedViewers - PlayerManager.Instance.CurrentViewers) * viewersAdjustmentPercentage/100f;


            // Update the viewer count
            PlayerManager.Instance.CurrentViewers = Mathf.RoundToInt(Mathf.Max(0, PlayerManager.Instance.CurrentViewers + adjustment));


            // Wait for the next update
            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator SimulateViewersNoise()
    {
        while (true)
        {
            while(!GameManager.Instance.isStreaming)
            {
                yield return null;
            }
            float interval = Random.Range(minUpdateNoiseInterval, maxUpdateNoiseInterval);
            // Calculate the noise percentage
            float noise = Random.Range(-viewersNoisePercentage, viewersNoisePercentage)/100f;

            // Update the viewer count
            PlayerManager.Instance.CurrentViewers = Mathf.FloorToInt(Mathf.Max(0, PlayerManager.Instance.CurrentViewers * (1 + noise)));

            // Wait for the next update
            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator SimulateViewers()
    {
        while (true)
        {
            while(!GameManager.Instance.isStreaming)
            {
                yield return null;
            }
            // Gradually adjust baseline viewers toward current viewers
            float adjustRange = (PlayerManager.Instance.CurrentViewers - BaselineViewers) * baselineChangePercentage/100f;
            if(adjustRange > 0)
            {
                adjustRange = Random.Range(0, adjustRange);
            }
            else
            {
                adjustRange = Random.Range(adjustRange, 0);
            }
            BaselineViewers = Mathf.FloorToInt(Mathf.Max(0, BaselineViewers + adjustRange));


            // Calculate expected viewers

            expectedViewers = Mathf.FloorToInt(BaselineViewers * (1 + PlayerManager.Instance.CurrentHype));
             


            // Wait for the next update
            yield return new WaitForSeconds(baselineUpdateInterval); // Adjust interval as needed
        }
    }




}
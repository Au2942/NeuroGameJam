using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSubscriptionsHandler : MonoBehaviour
{

    [SerializeField] public float massSubAmount = 0.01f; 
    [SerializeField] public float massSubAmountNoise = 0.1f; //10%
    [SerializeField] public float singleSubChance = 0.1f; //10%
    [SerializeField] public float bonusFactor = 5f; 
    [SerializeField] float minMassSubInterval = 5f;
    [SerializeField] float maxMassSubInterval = 10f;
    [SerializeField] float minSingleSubInterval = 1f;
    [SerializeField] float maxSingleSubInterval = 5f;

    public void AddSubscriptions(int value)
    {
        PlayerManager.Instance.Subscriptions += value;
        PlayerManager.Instance.RemainingStreamTime += PlayerManager.Instance.StreamTimeIncrease * value;
    }

    public IEnumerator SimulateSingleSubscription()
    {
        while (true)
        {
            if(!GameManager.Instance.isStreaming)
            {
                yield return null;
            }

            if(Random.value < singleSubChance)
            {
                AddSubscriptions(1);
                if (Random.value < 0.9f)
                {
                    float giftAmountChance = Random.value;
                    if (giftAmountChance < 0.6f)
                    {
                        AddSubscriptions(1);
                    }
                    else if (giftAmountChance < 0.85f)
                    {
                        AddSubscriptions(5);
                    }
                    else if (giftAmountChance < 0.95f)
                    {
                        AddSubscriptions(10);
                    }
                    else if (giftAmountChance < 0.98f)
                    {
                        AddSubscriptions(20);
                    }
                    else if (giftAmountChance < 0.995f)
                    {
                        AddSubscriptions(50);
                    }
                    else
                    {
                        AddSubscriptions(100);
                    }
                }
            }


            float bonus = PlayerManager.Instance.bonus;
            float interval = Random.Range(minSingleSubInterval, maxSingleSubInterval);
            if(bonus > 0)
            {
                interval /= bonus * bonusFactor;
            }
            else if(bonus < 0)
            {
                interval *= -bonus * bonusFactor;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator SimulateMassSubscription()
    {
        while (true)
        {
            if(!GameManager.Instance.isStreaming)
            {
                yield return null;
            }


            int expectedSubs = Mathf.RoundToInt(PlayerManager.Instance.CurrentViewers * massSubAmount);
            expectedSubs = Mathf.RoundToInt(expectedSubs * (1 + Random.Range(-massSubAmountNoise, massSubAmountNoise)));

            expectedSubs = Mathf.Max(0, expectedSubs);
            AddSubscriptions(expectedSubs);
            
            
            float bonus = PlayerManager.Instance.bonus;
            float interval = Random.Range(minMassSubInterval, maxMassSubInterval);
            if(bonus > 0)
            {
                interval /= bonus * bonusFactor;
            }
            else if(bonus < 0)
            {
                interval *= -bonus * bonusFactor;
            }

            yield return new WaitForSeconds(interval);
            
        }
    }



}
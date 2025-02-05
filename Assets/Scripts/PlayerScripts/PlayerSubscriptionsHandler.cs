using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSubscriptionsHandler : MonoBehaviour
{

    [SerializeField] public float massSubAmount = 0.01f; 
    [SerializeField] public float massSubAmountNoise = 0.1f; //10%
    [SerializeField] public float singleSubChance = 0.1f; //10%
    [SerializeField] public float interestsFactor = 5f; 
    [SerializeField] float minMassSubInterval = 5f;
    [SerializeField] float maxMassSubInterval = 10f;
    [SerializeField] float minSingleSubInterval = 1f;
    [SerializeField] float maxSingleSubInterval = 5f;

    public event System.Action<int> OnGainSubscriptionsEvent;


    public void AddSubscriptions(int value)
    {
        PlayerManager.Instance.Subscriptions += value;
        PlayerManager.Instance.IncreaseStreamTime(value);
        OnGainSubscriptionsEvent?.Invoke(value);
    }
    public IEnumerator AddSubscriptionsAfterPopup(int value)
    {
        PlayerManager.Instance.Subscriptions += value;
        PlayerManager.Instance.IncreaseStreamTime(value);
        yield return StartCoroutine(PlayerManager.Instance.StatUI.SpawnSubPopupNumber(value));
        PlayerManager.Instance.StatUI.ForceSyncRemainingStreamTime();
        OnGainSubscriptionsEvent?.Invoke(value);
    }

    public IEnumerator SimulateSingleSubscription()
    {
        while (true)
        {
            while(GameManager.Instance.isPause)
            {
                yield return null;
            }

            if(Random.value < singleSubChance)
            {
                StartCoroutine(AddSubscriptionsAfterPopup(1));
                if (Random.value < 0.9f)
                {
                    float giftAmountChance = Random.value;
                    if (giftAmountChance < 0.6f)
                    {
                        StartCoroutine(AddSubscriptionsAfterPopup(1));
                    }
                    else if (giftAmountChance < 0.85f)
                    {
                        StartCoroutine(AddSubscriptionsAfterPopup(5));
                    }
                    else if (giftAmountChance < 0.95f)
                    {
                        StartCoroutine(AddSubscriptionsAfterPopup(10));
                    }
                    else if (giftAmountChance < 0.98f)
                    {
                        StartCoroutine(AddSubscriptionsAfterPopup(20));
                    }
                    else if (giftAmountChance < 0.995f)
                    {
                        StartCoroutine(AddSubscriptionsAfterPopup(50));
                    }
                    else
                    {
                        StartCoroutine(AddSubscriptionsAfterPopup(100));
                    }
                }
            }


            float interests = PlayerManager.Instance.CurrentInterests;
            float interval = Random.Range(minSingleSubInterval, maxSingleSubInterval);
            if(interests > 0)
            {
                interval /= interests * interestsFactor;
            }
            else if(interests < 0)
            {
                interval *= -interests * interestsFactor;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator SimulateMassSubscription()
    {
        while (true)
        {
            while(GameManager.Instance.isPause || PlayerManager.Instance.State == PlayerManager.PlayerState.sleep)
            {
                yield return null;
            }


            int expectedSubs = Mathf.RoundToInt(PlayerManager.Instance.CurrentViewers * massSubAmount);
            expectedSubs = Mathf.RoundToInt(expectedSubs * (1 + Random.Range(-massSubAmountNoise, massSubAmountNoise)));

            expectedSubs = Mathf.Max(0, expectedSubs);
            StartCoroutine(AddSubscriptionsAfterPopup(expectedSubs));
            float interests = PlayerManager.Instance.CurrentInterests;
            float interval = Random.Range(minMassSubInterval, maxMassSubInterval);
            float modifier = Mathf.Abs(interests * interestsFactor);
            if(interests > 0)
            {
                interval /= 1+modifier;
            }
            else if(interests < 0)
            {
                interval *= 1+modifier;
            }

            yield return new WaitForSeconds(interval);
            
        }
    }



}
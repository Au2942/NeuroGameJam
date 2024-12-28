using UnityEngine;
using System.Collections.Generic;

public class StreamSelector : MonoBehaviour
{
    [SerializeField] private StreamCardSO[] streamCards;
    [SerializeField] private GameObject streamSelectorLayout;
    [SerializeField] private GameObject streamCard;
    [SerializeField] private int cardCount = 2;

    private List<GameObject> streamCardList = new List<GameObject>();
    private List<StreamCardSO> selectedStreams = new List<StreamCardSO>(); 
    private List<StreamCardSO> availableStreams = new List<StreamCardSO>();

    void Start()
    {
        availableStreams.AddRange(streamCards);
        OpenUI();
    }

    public void OpenUI()
    {
        streamSelectorLayout.SetActive(true);
        Setup();
    }

    private void Setup()
    {
        availableStreams.AddRange(selectedStreams);
        selectedStreams.Clear();
        for(int i = streamCardList.Count; i < cardCount; i++)
        {
            GameObject newStreamCard = Instantiate(streamCard, streamSelectorLayout.transform);
            newStreamCard.GetComponent<StreamCard>().OnSelect += (stream) => AddStream(stream);
            streamCardList.Add(newStreamCard);
        }
        foreach(GameObject streamCard in streamCardList)
        {
            int randomIndex = Random.Range(0, availableStreams.Count);
            streamCard.GetComponent<StreamCard>().Setup(availableStreams[randomIndex]);
            availableStreams.RemoveAt(randomIndex);
            selectedStreams.Add(streamCards[randomIndex]);
        }

    }

    private void AddStream(StreamSO stream)
    {
        TimelineManager.Instance.SetStream(stream);
        CloseUI();
    }

    public void CloseUI()
    {
        streamSelectorLayout.SetActive(false);
    }

}
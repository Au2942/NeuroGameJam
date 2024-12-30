using UnityEngine;
using System.Collections.Generic;

public class StreamSelector : MonoBehaviour
{
    [SerializeField] private StreamCardSO[] streamCards;
    [SerializeField] private GameObject streamSelectionLayout;
    [SerializeField] private GameObject streamCardLayout;
    [SerializeField] private GameObject streamCard;
    [SerializeField] private int cardCount = 2;
    private List<GameObject> streamCardList = new List<GameObject>();
    private List<StreamCardSO> selectedStreams = new List<StreamCardSO>(); 
    private List<StreamCardSO> availableStreams = new List<StreamCardSO>();

    private StreamSO previousStream;

    void Start()
    {
        availableStreams.AddRange(streamCards);
    }

    public void OpenUI()
    {
        streamSelectionLayout.SetActive(true);
        Setup();
    }

    private void Setup()
    {
        List<StreamCardSO> selectableStreams = new List<StreamCardSO>(streamCards);
        selectedStreams.Clear();
        for(int i = streamCardList.Count; i < cardCount; i++)
        {
            GameObject newStreamCard = Instantiate(streamCard, streamCardLayout.transform);
            newStreamCard.GetComponent<StreamCard>().OnSelect += (stream) => AddStream(stream);
            streamCardList.Add(newStreamCard);
        }
        foreach(GameObject streamCard in streamCardList)
        {
            int randomIndex = Random.Range(0, selectableStreams.Count);
            streamCard.GetComponent<StreamCard>().Setup(selectableStreams[randomIndex]);
            selectedStreams.Add(selectableStreams[randomIndex]);
            selectableStreams.RemoveAt(randomIndex);
        }
    }

    private void AddStream(StreamSO stream)
    {
        if(previousStream != null)
        {
            if(previousStream.streamName == stream.streamName)
            {
                PlayerManager.Instance.Debuff += 0.1f;
            }
            else
            {
                PlayerManager.Instance.Buff += 0.1f;
            }
        }


        previousStream = stream;
        GameManager.Instance.SetStream(stream);
        GameManager.Instance.PrepareStream();
        CloseUI();
    }

    public void CloseUI()
    {
        streamSelectionLayout.SetActive(false);
    }

}

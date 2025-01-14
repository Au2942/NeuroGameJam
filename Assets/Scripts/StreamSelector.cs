using UnityEngine;
using System.Collections.Generic;

public class StreamSelector : MonoBehaviour
{
    public static StreamSelector Instance;
    [SerializeField] private StreamCardSO[] streamCards;
    [SerializeField] private GameObject streamSelectionLayout;
    [SerializeField] private GameObject streamCardLayout;
    [SerializeField] private GameObject streamCard;
    [SerializeField] private int cardCount = 2;
    [SerializeField] public float hypeIncrease = 0.5f;
    private List<GameObject> streamCardList = new List<GameObject>();
    private List<StreamCardSO> selectedStreams = new List<StreamCardSO>(); 
    private List<StreamCardSO> availableStreams = new List<StreamCardSO>();

    public bool isOpen {get; private set;} = false;

    private bool isCardSet = false;

    private StreamSO previousStream;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        availableStreams.AddRange(streamCards);
    }

    public void OpenUI()
    {
        GameManager.Instance.StopStream();
        Time.timeScale = 0;
        isOpen = true;
        streamSelectionLayout.SetActive(true);
        if(!isCardSet)
        {
            Setup();
        }
    }

    public void Setup()
    {
        List<StreamCardSO> selectableStreams = new List<StreamCardSO>(streamCards);
        selectedStreams.Clear();
        if (previousStream != null)
        {
            selectableStreams.RemoveAll(stream => stream.stream == previousStream);
        }

        for(int i = streamCardList.Count; i < cardCount; i++)
        {
            GameObject newStreamCard = Instantiate(streamCard, streamCardLayout.transform);
            newStreamCard.GetComponent<StreamCard>().OnSelect += (stream) => SetNewStream(stream);
            streamCardList.Add(newStreamCard);
        }
        foreach(GameObject streamCard in streamCardList)
        {
            int randomIndex = Random.Range(0, selectableStreams.Count);
            streamCard.GetComponent<StreamCard>().Setup(selectableStreams[randomIndex]);
            selectedStreams.Add(selectableStreams[randomIndex]);
            selectableStreams.RemoveAt(randomIndex);
        }
        isCardSet = true;
    }

    private void SetNewStream(StreamSO stream)
    {
        GameManager.Instance.EndStream();
        previousStream = stream;
        GameManager.Instance.StartNewStream(stream);
        PlayerManager.Instance.TargetHype = PlayerManager.Instance.CurrentHype + hypeIncrease;
        isCardSet = false;
        CloseUI();
    }

    public void CloseUI()
    {
        GameManager.Instance.ContinueStream();
        Time.timeScale = 1;
        isOpen = false;
        streamSelectionLayout.SetActive(false);
    }

    void OnDestroy()
    {
        foreach(GameObject streamCardObject in streamCardList)
        {
            StreamCard streamCard = streamCardObject.GetComponent<StreamCard>();
            if (streamCard != null)
            {
                streamCard.OnSelect -= (stream) => SetNewStream(stream);
            }
        }
    }
}

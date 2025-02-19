using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PlaybackTimeline : MonoBehaviour
{
    public Playback BasePlayback = new Playback();
    public Playback ErrorPlayback = new Playback();
    public Scrollbar PlaybackScroll;
    public UIEventHandler PlaybackScrollDragDetector;
    public bool IsDraggingScroll = false;
    public bool IsPaused = false;
    public TextMeshProUGUI CurrentTimeText;
    public TextMeshProUGUI DurationText;
    public ColorStripes ColorStripesEffect; //for visualizing corrupted parts
    public Color HealthyColor = Color.white ;
    public Color CorruptedColor = Color.red;
    public Color RepairedColor = Color.cyan;
    UnityEngine.Events.UnityAction<float> playbackTimeDelegate;
    System.Action<float, float> onBaseAddCorruptPartDelegate;
    System.Action<float, float> onBaseRemoveCorruptPartDelegate;
    System.Action<PointerEventData> onBeginDragScrollDelegate;
    System.Action<PointerEventData> onEndDragScrollDelegate;
    public float NormalizedDeltaTime => BasePlayback.NormalizeTime(Time.deltaTime);
    private Coroutine addCorruptedSegmentRoutine;
    private Coroutine addErrorSegmentRoutine;


    void Awake()
    {
        BasePlayback ??= new Playback();  
        ErrorPlayback ??= new Playback(); 
        playbackTimeDelegate = (t) => BasePlayback.SetCurrentPlaybackTime(t);
        onBaseRemoveCorruptPartDelegate = (s, e) => SetColorStripesChunk(s, e, 0);
        onBaseAddCorruptPartDelegate = (s, e) => SetColorStripesChunk(s, e, 1);
        onBeginDragScrollDelegate = (t) => SetDraggingScroll(true);
        onEndDragScrollDelegate = (t) => SetDraggingScroll(false);

        if(PlaybackScroll != null) PlaybackScroll.onValueChanged.AddListener(playbackTimeDelegate);
        BasePlayback.OnCorruptPartAdd += onBaseAddCorruptPartDelegate;
        BasePlayback.OnCorruptPartRemove += onBaseRemoveCorruptPartDelegate;
        PlaybackScrollDragDetector.OnBeginDragEvent += onBeginDragScrollDelegate;
        PlaybackScrollDragDetector.OnEndDragEvent += onEndDragScrollDelegate;
    }
    void OnDestroy()
    {
        PlaybackScroll.onValueChanged.RemoveListener(playbackTimeDelegate);
        BasePlayback.OnCorruptPartAdd -= onBaseAddCorruptPartDelegate;
        BasePlayback.OnCorruptPartRemove -= onBaseRemoveCorruptPartDelegate;
        PlaybackScrollDragDetector.OnBeginDragEvent -= onBeginDragScrollDelegate;
        PlaybackScrollDragDetector.OnEndDragEvent -= onEndDragScrollDelegate;
    }

    public bool HasCorruptedSegments()
    {
        return BasePlayback.CorruptedSegments.Count > 0;
    }

    public void SetDraggingScroll(bool state)
    {
        IsDraggingScroll = state;
    }

    public void PausePlayback(bool state)
    {
        IsPaused = state;
    }

    public void LockScrollbar(bool state)
    {
        PlaybackScroll.interactable = !state;
    }

    public bool RoutineIsRunning()
    {
        return addCorruptedSegmentRoutine != null || addErrorSegmentRoutine != null;
    }

    public float GetCurrentPlaybackTime()
    {
        return BasePlayback.CurrentPlaybackTimePercentage;
    }

    public float NormalizeTime(float time)
    {
        return BasePlayback.NormalizeTime(time);
    }

    public void SetPlaybackScrollValue(float time)
    {
        if(IsDraggingScroll) return;
        PlaybackScroll.value = time > 1 ? time%1 : time;
    }

    public PlaybackSegment CheckOverlappedSegment(float point)
    {
        return BasePlayback.GetContainedCorruptedSegment(point);
    }

    public void StopAddCorruptedSegmentRoutine()
    {
        if(addCorruptedSegmentRoutine != null) StopCoroutine(addCorruptedSegmentRoutine);
        if(addErrorSegmentRoutine != null) StopCoroutine(addErrorSegmentRoutine);
    }
    
    public void AddCorruptedSegment(float start, float duration, bool errorPlayback = false)
    {
        if(errorPlayback) ErrorPlayback.AddCorruptedSegment(start, duration);
        else BasePlayback.AddCorruptedSegment(start, duration);
    }

    //<summary> Adds a corrupted segment over specified playback normalized duration </summary>
    public void AddCorruptedSegmentOvertime(float duration)
    {
        if(addCorruptedSegmentRoutine != null) StopCoroutine(addCorruptedSegmentRoutine);
        addCorruptedSegmentRoutine = StartCoroutine(AddCorruptedSegmentRoutine(duration));
    }

    //<summary> Add error segment over specified playback normalized duration </summary>
    public void AddErrorSegmentOvertime(float duration)
    {
        if(addErrorSegmentRoutine != null) StopCoroutine(addErrorSegmentRoutine);
        addErrorSegmentRoutine = StartCoroutine(AddCorruptedSegmentRoutine(duration, true));
    }

    private IEnumerator AddCorruptedSegmentRoutine(float duration, bool errorPlayback = false)
    {
        //Debug.Log("Adding corrupted segment");
        float start = BasePlayback.CurrentPlaybackTimePercentage;
        float normalizedDuration = BasePlayback.NormalizeTime(duration);
        float elapsedDuration = 0;
        while(normalizedDuration > elapsedDuration)
        {
            yield return null;
            AddCorruptedSegment(start, elapsedDuration, errorPlayback);
            elapsedDuration += NormalizedDeltaTime;
        }
        AddCorruptedSegment(start, normalizedDuration, errorPlayback); 
    }


    public void RemoveCorruptedSegment(float amount)
    {
        float point = BasePlayback.CurrentPlaybackTimePercentage;
        float normalizedAmount = BasePlayback.NormalizeTime(amount);
        BasePlayback.RemoveCorruptedPart(point, normalizedAmount);
        ErrorPlayback.RemoveCorruptedPart(point, normalizedAmount);
    }

    void Start()
    {
        if(ColorStripesEffect != null)
        {
            ColorStripesEffect.SetColor(0, HealthyColor);
            ColorStripesEffect.SetColor(1, CorruptedColor);
            ColorStripesEffect.SetColor(2, RepairedColor);
        }
    }

    // public void ClearErrorPlayback()
    // {
    //     // ErrorPlayback.CorruptedSegments.Clear();
    //     // foreach(PlaybackSegment segment in ActivePlayback.CorruptedSegments)
    //     // {
    //     //     SetColorStripesChunk(segment.Start, segment.End, 1);
    //     // }
    // }

    public void SetupPlaybackTimeline(float duration)
    {   
        BasePlayback.PlaybackDuration = duration;
        ErrorPlayback.PlaybackDuration = duration;
    }


    public void SetColorStripesChunk(float start, float end, int value)
    {
        int startIndex = Mathf.RoundToInt(start * ColorStripesEffect.Frequency);
        int endIndex = Mathf.RoundToInt(end * ColorStripesEffect.Frequency);
        ColorStripesEffect.SetChunkRange(startIndex, endIndex, value);
    }

    void Update()
    {
        if(!IsPaused)
        {
            PlaybackScroll.value += BasePlayback.NormalizeTime(Time.deltaTime);
            PlaybackScroll.value = PlaybackScroll.value > 1 ? PlaybackScroll.value%1 : PlaybackScroll.value;    
        }
        CurrentTimeText.text = TimescaleManager.Instance.FormatTimeString(BasePlayback.AbsoluteCurrentTime());
        DurationText.text = TimescaleManager.Instance.FormatTimeString(BasePlayback.PlaybackDuration);
    }


}

#region Playback
[System.Serializable]
public class Playback
{
    public float CurrentPlaybackTimePercentage = 0f; //0 to 1
    public float PlaybackDuration = 0f;
    public List<PlaybackSegment> CorruptedSegments = new List<PlaybackSegment>();
    public event System.Action<float, float> OnCorruptPartAdd;
    public event System.Action<float, float> OnCorruptPartRemove;

    public Playback() {}
    public Playback(float duration)
    {
        PlaybackDuration = duration;
    }
    public void SetCurrentPlaybackTime(float percentage)
    {
        CurrentPlaybackTimePercentage = percentage;
    }

    public bool InCorruptedPart()
    {
        foreach(PlaybackSegment segment in CorruptedSegments)
        {
            if(CurrentPlaybackTimePercentage >= segment.Start && CurrentPlaybackTimePercentage <= segment.End)
            {
                return true;
            }
        }
        return false;
    }
    public float AbsoluteCurrentTime()
    {
        return AbsoluteTime(CurrentPlaybackTimePercentage);
    }
    public float AbsoluteTime(float percentage)
    {
        return percentage * PlaybackDuration;
    }

    public float NormalizeTime(float absoluteTime)
    {
        return absoluteTime / PlaybackDuration;
    }

    public float GetCorruptedPercentage()
    {
        float percentage = 0;
        foreach(PlaybackSegment segment in CorruptedSegments)
        {
            percentage += segment.Duration();
        }
        return percentage;
    }

    public float GetCorruptedSegmentsDuration()
    {
        return AbsoluteTime(GetCorruptedPercentage());
    }

    public PlaybackSegment GetContainedCorruptedSegment(float point)
    {
        foreach(PlaybackSegment segment in CorruptedSegments)
        {
            if(point >= segment.Start && point <= segment.End)
            {
                return segment;
            }
        }
        return null;
    }

    public void AddCorruptedSegment(float start, float duration)
    {
        if (GetCorruptedPercentage() >= 1) return;
        float end = start + duration;
        if(end <= 1)
        {
           AddCorruptedSegment(new PlaybackSegment(start, end)); 
        }
        else
        {
            AddCorruptedSegment(new PlaybackSegment(start, 1));
            AddCorruptedSegment(new PlaybackSegment(0, end%1));
        }
    }

    public void AddCorruptedSegment(PlaybackSegment checkSegment)
    {
        if (GetCorruptedPercentage() >= 1 || CorruptedSegments.Contains(checkSegment)) return;

        float segmentDuration = checkSegment.Duration();
        float newStart = checkSegment.Start;
        float newEnd = checkSegment.End;

        // float overflowStart = 0;
        // float overflowEnd = 0;
        List<PlaybackSegment> toRemove = new List<PlaybackSegment>();

        foreach (PlaybackSegment segment in CorruptedSegments)
        {
            // Check if the new segment overlaps with existing ones
            if (segment.IsOverlapping(checkSegment)) 
            {
                // Merge logic
                if (checkSegment.Start >= segment.Start) 
                {
                    newStart = segment.Start;
                }
                else 
                {
                    newEnd = segment.End;
                }

                toRemove.Add(segment); // Mark for removal
            }
        }

        // Ensure wrapping around if needed
        // if (newEnd > 1f)
        // {
        //     float overflow = newEnd - 1f;
        //     newEnd = 1f;
        //     overflowStart = 0;
        //     overflowEnd = overflow;
        // }

        // if (newStart < 0)
        // {
        //     float overflow = -newStart;
        //     newStart = 0;
        //     overflowStart = 1f - overflow;
        //     overflowEnd = 1f;
        // }

        // Remove merged segments
        foreach (PlaybackSegment segment in toRemove)
        {
            CorruptedSegments.Remove(segment);
        }

        // Add the newly extended range
        PlaybackSegment newSegment = new PlaybackSegment(newStart, newEnd);
        CorruptedSegments.Add(newSegment);
        CorruptedSegments.Sort((a, b) => a.Start.CompareTo(b.Start));

        // // Add the overflow part
        // if(overflowStart != overflowEnd)
        // {
        //     AddCorruptedSegment(overflowStart, overflowEnd);
        // }

        OnCorruptPartAdd?.Invoke(newStart, newEnd);
    }

    public void RemoveCorruptedPart(float point, float duration)
    {
        if (duration <= 0 || CorruptedSegments.Count == 0) return;

        float removeSegmentStart = point;
        float removeSegmentEnd = point + duration;
        float removeSegmentDuration = duration;
        // Find the segments that overlaps with the given range
        List<PlaybackSegment> targetSegments = new();

        foreach (PlaybackSegment segment in CorruptedSegments)
        {
            if (segment.IsOverlapping(removeSegmentStart, removeSegmentEnd))
            {
                targetSegments.Add(segment);
            }
        }

        // If no segment was found where start time falls within, skip
        if (targetSegments.Count == 0) return;

        foreach(PlaybackSegment targetSegment in targetSegments)
        {
            // Remove duration from the found segments within the given range

            if(removeSegmentStart < targetSegment.Start)
            {
                removeSegmentStart = targetSegment.Start;
                removeSegmentEnd = removeSegmentStart + removeSegmentDuration;
            }
            else if(removeSegmentEnd > targetSegment.End)
            {
                removeSegmentEnd = targetSegment.End;
                removeSegmentStart = removeSegmentEnd - removeSegmentDuration;
            }

            // Remove entire segment or adjust start/end as necessary

            if (targetSegment.Start >= removeSegmentStart && targetSegment.End <= removeSegmentEnd)
            {
                // Remove the entire segment if fully overlapped
                CorruptedSegments.Remove(targetSegment); 
            }
            else if (targetSegment.Start == removeSegmentStart)
            {
                // Shrink from the start
                targetSegment.Start = removeSegmentEnd;
                
            }
            else if (targetSegment.End == removeSegmentEnd)
            {
                // Shrink from the end
                targetSegment.End = removeSegmentStart;
            }
            else
            {
                // Split into two if the duration to remove is in the middle
                targetSegment.End = removeSegmentStart;
                AddCorruptedSegment(new PlaybackSegment(removeSegmentEnd, targetSegment.End));
            }
            removeSegmentDuration -= removeSegmentStart - removeSegmentEnd;
            OnCorruptPartRemove?.Invoke(removeSegmentStart, removeSegmentEnd);
        }
       
    }
}
#endregion

[System.Serializable]
public class PlaybackSegment
{
    public float Start;
    public float End;

    public PlaybackSegment(float start, float end)
    {
        Start = start;
        End = end;
    }

    public float Duration()
    {
        return End - Start;
    }

    public bool IsOverlapping(PlaybackSegment segment)
    {
        return Start <= segment.End && End >= segment.Start;
    }
    public bool IsOverlapping(float checkStart, float checkEnd)
    {
        return Start <= checkEnd && End >= checkStart;
    }

}
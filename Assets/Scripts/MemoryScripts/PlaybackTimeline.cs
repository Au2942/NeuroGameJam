using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlaybackTimeline : MonoBehaviour
{
    public Playback ActivePlayback;
    public Playback BasePlayback = new Playback();
    public Playback ErrorPlayback = new Playback();
    public Scrollbar PlaybackScroll;
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
    System.Action<float, float> onErrorRemoveCorruptPartDelegate;

    void Awake()
    {
        BasePlayback ??= new Playback();  
        ErrorPlayback ??= new Playback(); 
        playbackTimeDelegate = (t) => ActivePlayback.SetCurrentPlaybackTime(t);
        onBaseRemoveCorruptPartDelegate = (s, e) => {SetColorStripesChunk(s, e, 0); if(ActivePlayback == ErrorPlayback) ErrorPlayback.RemoveCorruptPart(s, e);};
        onBaseAddCorruptPartDelegate = (s, e) => {SetColorStripesChunk(s, e, 1); if(ActivePlayback == ErrorPlayback) ErrorPlayback.AddCorruptPart(s, e);};
        onErrorRemoveCorruptPartDelegate = (s, e) => {if(ActivePlayback == ErrorPlayback) SetColorStripesChunk(s, e, 2);};

        if(PlaybackScroll != null) PlaybackScroll.onValueChanged.AddListener(playbackTimeDelegate);
        BasePlayback.OnCorruptPartAdd += onBaseAddCorruptPartDelegate;
        BasePlayback.OnCorruptPartRemove += onBaseRemoveCorruptPartDelegate;
        ErrorPlayback.OnCorruptPartRemove += onErrorRemoveCorruptPartDelegate;

        ActivePlayback = BasePlayback;
    }

    public void PausePlayback(bool state)
    {
        IsPaused = state;
    }

    public void LockScrollbar(bool state)
    {
        PlaybackScroll.interactable = !state;
    }

    public void SetActivePlaybackTime(float time)
    {
        PlaybackScroll.value = time;
    }

    public void SetupErrorPlayback()
    {
        ErrorPlayback.PlaybackDuration = BasePlayback.PlaybackDuration;
        ErrorPlayback.CorruptedParts.Clear();
        foreach(PlaybackSegment segment in BasePlayback.CorruptedParts)
        {
            ErrorPlayback.AddCorruptPart(segment);
        }
        ErrorPlayback.SetCurrentPlaybackTime(BasePlayback.CurrentPlaybackTimePercentage);
        ActivePlayback = ErrorPlayback;
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

    public void ClearErrorPlayback()
    {
        ErrorPlayback.CorruptedParts.Clear();
        foreach(PlaybackSegment segment in BasePlayback.CorruptedParts)
        {
            SetColorStripesChunk(segment.Start, segment.End, 1);
        }
        ActivePlayback = BasePlayback;
    }

    public void SetupPlaybackTimeline(float duration)
    {   
        BasePlayback.PlaybackDuration = duration;
        ErrorPlayback.PlaybackDuration = duration;
    }

    void OnDestroy()
    {
        PlaybackScroll.onValueChanged.RemoveListener(playbackTimeDelegate);
        BasePlayback.OnCorruptPartAdd -= onBaseAddCorruptPartDelegate;
        BasePlayback.OnCorruptPartRemove -= onBaseRemoveCorruptPartDelegate;
    }
    public void SetColorStripesChunk(float start, float end, int value)
    {
        int startIndex = Mathf.FloorToInt(start * ColorStripesEffect.Frequency);
        int endIndex = Mathf.FloorToInt(end * ColorStripesEffect.Frequency);
        ColorStripesEffect.SetChunkRange(startIndex, endIndex, value);
    }

    void Update()
    {
        if(!IsPaused)
        {
            PlaybackScroll.value += ActivePlayback.NormalizeTime(Time.deltaTime);
            PlaybackScroll.value = PlaybackScroll.value > 1 ? PlaybackScroll.value%1 : PlaybackScroll.value;    
        }
        CurrentTimeText.text = TimescaleManager.Instance.FormatTimeString(ActivePlayback.AbsoluteCurrentTime());
        DurationText.text = TimescaleManager.Instance.FormatTimeString(ActivePlayback.PlaybackDuration);
    }


}


[System.Serializable]
public class Playback
{
    public float CurrentPlaybackTimePercentage = 0f; //0 to 1
    public float PlaybackDuration = 0f;
    public List<PlaybackSegment> CorruptedParts = new List<PlaybackSegment>();
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
        foreach(PlaybackSegment segment in CorruptedParts)
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
        foreach(PlaybackSegment segment in CorruptedParts)
        {
            percentage += segment.Duration();
        }
        return percentage;
    }

    public float GetCorruptedPartsDuration()
    {
        return AbsoluteTime(GetCorruptedPercentage());
    }

    public PlaybackSegment AddRandomCorruptPart(float percentage)
    {
        if (GetCorruptedPercentage() >= 1) return null;

        float start = Random.Range(0f, 1f - percentage);
        float end = start + percentage;
        return AddCorruptPart(start, end);
    }

    public PlaybackSegment AddCorruptPart(float start, float end)
    {
        if (GetCorruptedPercentage() >= 1) return null;
        if(start < 0f) 
        {
            start = 0f;
        }
        if(end > 1f)
        {
            end = 1f;
        }
        PlaybackSegment newSegment = new PlaybackSegment(start, end);
        return AddCorruptPart(newSegment);
    }

    public PlaybackSegment AddCorruptPart(PlaybackSegment checkSegment)
    {
        if (GetCorruptedPercentage() >= 1 || CorruptedParts.Contains(checkSegment)) return null;

        float segmentDuration = checkSegment.Duration();
        float newStart = checkSegment.Start;
        float newEnd = checkSegment.End;
        float overflowStart = 0;
        float overflowEnd = 0;
        List<PlaybackSegment> toRemove = new List<PlaybackSegment>();

        foreach (PlaybackSegment segment in CorruptedParts)
        {
            // Check if the new segment overlaps with existing ones
            if (!(newEnd < segment.Start || newStart > segment.End)) 
            {
                // Merge logic: Extend in the same direction as checkSegment
                if (checkSegment.Start >= segment.Start) 
                {
                    // Extend forward by segmentDuration
                    newStart = segment.Start;
                    newEnd = segment.End + segmentDuration;
                }
                else 
                {
                    // Extend backward by segmentDuration
                    newEnd = segment.End;
                    newStart = segment.Start - segmentDuration;
                }

                toRemove.Add(segment); // Mark for removal
            }
        }

        // Ensure wrapping around if needed
        if (newEnd > 1f)
        {
            float overflow = newEnd - 1f;
            newEnd = 1f;
            overflowStart = 0;
            overflowEnd = overflow;
        }

        if (newStart < 0)
        {
            float overflow = -newStart;
            newStart = 0;
            overflowStart = 1f - overflow;
            overflowEnd = 1f;
        }

        // Remove merged segments
        foreach (PlaybackSegment segment in toRemove)
        {
            CorruptedParts.Remove(segment);
        }

        // Add the newly extended range
        PlaybackSegment newSegment = new PlaybackSegment(newStart, newEnd);
        CorruptedParts.Add(newSegment);
        CorruptedParts.Sort((a, b) => a.Start.CompareTo(b.Start));

        // Add the overflow part
        if(overflowStart != overflowEnd)
        {
            AddCorruptPart(overflowStart, overflowEnd);
        }

        OnCorruptPartAdd?.Invoke(newSegment.Start, newSegment.End);
        return newSegment;
    }

    public void RemoveCorruptPart(float point, float percentage)
    {
        float removeSegmentDuration = percentage;
        if (removeSegmentDuration <= 0 || CorruptedParts.Count == 0) return;

        // Find the segment where the point is located
        PlaybackSegment targetSegment = null;
        foreach (PlaybackSegment segment in CorruptedParts)
        {
            if (point >= segment.Start && point <= segment.End)
            {
                targetSegment = segment;
                break; // Found the segment, no need to continue
            }
        }

        // If no segment was found where start time falls within, skip
        if (targetSegment == null) return;

        // Remove duration from the found segment within the given range
        float removeSegmentStart = point - percentage/2;
        float removeSegmentEnd = point + percentage/2;

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
            CorruptedParts.Remove(targetSegment);
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
            AddCorruptPart(new PlaybackSegment(removeSegmentEnd, targetSegment.End));
        }

        OnCorruptPartRemove?.Invoke(removeSegmentStart, removeSegmentEnd);
    }
}

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

}
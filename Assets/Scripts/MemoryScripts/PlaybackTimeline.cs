using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlaybackTimeline : MonoBehaviour
{
    public Playback Playback;
    public Scrollbar PlaybackScroll;
    public TextMeshProUGUI CurrentTimeText;
    public TextMeshProUGUI DurationText;
    public ColorStripes ColorStripesEffect; //for visualizing corrupted parts
    UnityEngine.Events.UnityAction<float> playbackTimeDelegate;
    System.Action<float, float> onAddCorruptPartDelegate;
    System.Action<float, float> onRemoveCorruptPartDelegate;

    public void SetupPlaybackTimeline(Playback playback)
    {   
        if(Playback != null)
        {
            Playback = playback;

            playbackTimeDelegate = (t) => Playback.SetCurrentPlaybackTime(t);
            onAddCorruptPartDelegate = (s, e) => SetColorStripesChunk(s, e, 1);
            onRemoveCorruptPartDelegate = (s, e) => SetColorStripesChunk(s, e, 0);

            PlaybackScroll.onValueChanged.AddListener(playbackTimeDelegate);
            Playback.OnCorruptPartAdd += onAddCorruptPartDelegate;
            Playback.OnCorruptPartRemove += onRemoveCorruptPartDelegate;
        }
    }

    public void SetColorStripesChunk(float start, float end, int value)
    {
        int startIndex = Mathf.FloorToInt(start / Playback.PlaybackDuration * ColorStripesEffect.Frequency);
        int endIndex = Mathf.FloorToInt(end / Playback.PlaybackDuration * ColorStripesEffect.Frequency);
        ColorStripesEffect.SetChunkRange(startIndex, endIndex, value);
    }

    void OnEnable()
    {
        if(Playback != null)
        {
            playbackTimeDelegate = (t) => Playback.SetCurrentPlaybackTime(t);
            onAddCorruptPartDelegate = (s, e) => SetColorStripesChunk(s, e, 1);
            onRemoveCorruptPartDelegate = (s, e) => SetColorStripesChunk(s, e, 0);

            PlaybackScroll.onValueChanged.AddListener(playbackTimeDelegate);
            Playback.OnCorruptPartAdd += onAddCorruptPartDelegate;
            Playback.OnCorruptPartRemove += onRemoveCorruptPartDelegate;
        }
    }

    void OnDisable()
    {
        PlaybackScroll.onValueChanged.RemoveListener(playbackTimeDelegate);
        Playback.OnCorruptPartAdd -= onAddCorruptPartDelegate;
        Playback.OnCorruptPartRemove -= onRemoveCorruptPartDelegate;
    }

    void Update()
    {
        if(Playback != null)
        {
            CurrentTimeText.text = (Playback.CurrentPlaybackTime * TimescaleManager.Instance.displayTimeMultiplier).ToString("F2");
            DurationText.text = (Playback.PlaybackDuration * TimescaleManager.Instance.displayTimeMultiplier).ToString("F2");
        }   
    }

}


[System.Serializable]
public class Playback
{
    public float CurrentPlaybackTime = 0f;
    public float PlaybackDuration = 0f;
    public List<PlaybackSegment> CorruptedParts = new List<PlaybackSegment>();
    public event System.Action<float, float> OnCorruptPartAdd;
    public event System.Action<float, float> OnCorruptPartRemove;

    public Playback() {}
    public Playback(float duration)
    {
        PlaybackDuration = duration;
    }

    public float PlaybackTimePercentage()
    {
        return CurrentPlaybackTime / PlaybackDuration;
    }
    public float PlaybackTimestamp(float percentage)
    {
        return PlaybackDuration * percentage;
    }

    public void SetCurrentPlaybackTime(float percentage)
    {
        CurrentPlaybackTime = PlaybackTimestamp(percentage);
    }

    private float CorruptedPartsDuration()
    {
        float duration = 0;
        foreach(PlaybackSegment segment in CorruptedParts)
        {
            duration += segment.Duration();
        }
        return duration;
    }

    public PlaybackSegment AddRandomCorruptPart(float duration)
    {
        if (CorruptedPartsDuration() >= PlaybackDuration) return null;

        float start = Random.Range(0, PlaybackDuration - duration);
        float end = start + duration;
        return AddCorruptPart(start, end);
    }

    public PlaybackSegment AddCorruptPart(float start, float end)
    {
        if (CorruptedPartsDuration() >= PlaybackDuration) return null;
        if(start < 0) 
        {
            start = 0;
        }
        if(end > PlaybackDuration)
        {
            end = PlaybackDuration;
        }
        PlaybackSegment newSegment = new PlaybackSegment(start, end);
        return AddCorruptPart(newSegment);
    }


    public PlaybackSegment AddCorruptPart(PlaybackSegment checkSegment)
    {
        if (CorruptedPartsDuration() >= PlaybackDuration || CorruptedParts.Contains(checkSegment)) return null;

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
        if (newEnd > PlaybackDuration)
        {
            float overflow = newEnd - PlaybackDuration;
            newEnd = PlaybackDuration;
            overflowStart = 0;
            overflowEnd = overflow;
        }

        if (newStart < 0)
        {
            float overflow = -newStart;
            newStart = 0;
            overflowStart = PlaybackDuration - overflow;
            overflowEnd = PlaybackDuration;
        }

        // Remove merged segments
        foreach (PlaybackSegment segment in toRemove)
        {
            CorruptedParts.Remove(segment);
        }

        // Add the newly extended range
        PlaybackSegment newSegment = new PlaybackSegment(newStart, newEnd);
        CorruptedParts.Add(newSegment);

        // Add the overflow part
        if(overflowStart != overflowEnd)
        {
            AddCorruptPart(overflowStart, overflowEnd);
        }

        OnCorruptPartAdd?.Invoke(newSegment.Start, newSegment.End);
        return newSegment;
        
    }
    public void RemoveCorruptPart(float start, float end)
    {
        float duration = end - start;
        if (duration <= 0 || CorruptedParts.Count == 0 || start >= end) return;

        // Find the segment where the start time is located
        PlaybackSegment targetSegment = null;
        foreach (PlaybackSegment segment in CorruptedParts)
        {
            if (start >= segment.Start && start <= segment.End)
            {
                targetSegment = segment;
                break; // Found the segment, no need to continue
            }
        }

        // If no segment was found where start time falls within, skip
        if (targetSegment == null) return;

        // Remove duration from the found segment within the given range
        float segmentStart = Mathf.Max(targetSegment.Start, start);
        float segmentEnd = Mathf.Min(targetSegment.End, end);
        float segmentDuration = segmentEnd - segmentStart;

        if (segmentDuration > duration)
        {
            // Shrink the segment from the start
            targetSegment.Start += duration;
        }
        else
        {
            // Remove entire segment or adjust start/end as necessary

            if (targetSegment.Start == segmentStart && targetSegment.End == segmentEnd)
            {
                // Remove the entire segment if fully consumed
                CorruptedParts.Remove(targetSegment);
            }
            else if (targetSegment.Start == segmentStart)
            {
                // Shrink from the start
                targetSegment.Start = segmentEnd;
            }
            else if (targetSegment.End == segmentEnd)
            {
                // Shrink from the end
                targetSegment.End = segmentStart;
            }
            else
            {
                // Split into two if the duration to remove is in the middle
                targetSegment.End = segmentStart;
                AddCorruptPart(new PlaybackSegment(segmentEnd, targetSegment.End));
            }
        }
        OnCorruptPartRemove?.Invoke(segmentStart, segmentEnd);
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
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] public AudioSource SoundFXObjectPrefab;
    private int poolSize = 128;
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            
        }
    }

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        // Get the maximum number of virtual voices (channels) available
        //int maxVirtualChannels = AudioSettings.GetConfiguration().numVirtualVoices;

        // Set the pool size to the maximum number of virtual channels
        //poolSize = maxVirtualChannels;

        // Initialize the pool
        if(SoundFXObjectPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = Instantiate(SoundFXObjectPrefab, transform);
                audioSource.gameObject.SetActive(false);
                audioSourcePool.Enqueue(audioSource);
            }
        }

    }

    public AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource audioSource = audioSourcePool.Dequeue();
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }
        else
        {
            //Debug.LogWarning("SFXManager: No available audio sources in the pool.");
            return null;
        }
    }

    public void ReturnAudioSource(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.gameObject.transform.SetParent(transform);
            audioSource.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSource);
        }
    }

    public AudioSource PlaySoundFX(AudioClip clip, Transform transform, float volume = 0.5f)
    {
        AudioSource audioSource = GetAudioSource();
        if (audioSource != null)
        {
            audioSource.transform.position = transform.position;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.PlayOneShot(clip);

            // Return the audio source to the pool after it finishes playing
            StartCoroutine(ReturnToPoolAfterPlaying(audioSource, clip.length));
        }
        return audioSource;
    }

    public AudioSource PlaySoundFX(List<AudioClip> clips, Transform transform, float volume = 1f)
    {   
        int randomIndex = Random.Range(0, clips.Count);
        return PlaySoundFX(clips[randomIndex], transform, volume);
    }
    public AudioSource PlaySoundFX(List<AudioClip> clips, float volume = 1f)
    {
        int randomIndex = Random.Range(0, clips.Count);
        return PlaySoundFX(clips[randomIndex], transform, volume);
    }
    public AudioSource PlaySoundFX(AudioClip clip, float volume = 1f)
    {
        return PlaySoundFX(clip, transform, volume);
    }

    private IEnumerator<WaitForSeconds> ReturnToPoolAfterPlaying(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnAudioSource(audioSource);
    }
}
using UnityEngine;
using TMPro;

public class SwearWord : Interactable
{
    [SerializeField] private TextMeshProUGUI swearWordText;
    [SerializeField] private AudioClip filteredAudioClip;
    [SerializeField] private float moveSpeed = 50f;
    private Vector2 moveDirection;

    public override void OnSpawn()
    {
        base.OnSpawn();
        swearWordText.text = GenerateFakeCensoredSwearWord();
        moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void Update()
    {
        transform.localPosition += (Vector3)(moveDirection * Time.deltaTime * moveSpeed);
        ReachEdgeCheck();
    }

    private void ReachEdgeCheck()
    {
        if (transform.localPosition.x > 1536 / 2 || transform.localPosition.x < -1536 / 2 ||
            transform.localPosition.y > 864 / 2 || transform.localPosition.y < -864 / 2)
        {
            PlayerManager.Instance.TakeDamage(2);
            InteractableSpawner.ReturnObjectToPool(gameObject);
        }
    }

    
    private string GenerateFakeCensoredSwearWord()
    {
        string symbols = "!@#$%^&";
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        int length = Random.Range(4, 10); // Random length between 4 and 10
        char[] stringChars = new char[length];

        // Fill most of the string with symbols
        for (int i = 0; i < length - 2; i++)
        {
            stringChars[i] = symbols[Random.Range(0, symbols.Length)];
        }

        // Add 1-2 alphabet characters at random positions
        for (int i = length - 2; i < length; i++)
        {
            stringChars[i] = alphabet[Random.Range(0, alphabet.Length)];
        }

        // Shuffle the array to mix symbols and alphabet characters
        for (int i = 0; i < stringChars.Length; i++)
        {
            int j = Random.Range(0, stringChars.Length);
            char temp = stringChars[i];
            stringChars[i] = stringChars[j];
            stringChars[j] = temp;
        }

        return new string(stringChars);
    }

    public void PlayFilteredAudio()
    {
        SFXManager.Instance.PlaySoundFX(filteredAudioClip, transform);
    }

}

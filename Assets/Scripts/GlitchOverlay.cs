using UnityEngine;
using UnityEngine.UI;



public class GlitchOverlay : MonoBehaviour
{
    [SerializeField] private Image glitchEffect;
    [SerializeField] Material glitchMaterial;

    [SerializeField] private float maxBlock = 0.02f;
    [SerializeField] private float maxDrift = 0.1f;
    [SerializeField] private float maxJitter = 0.1f;
    [SerializeField] private float maxJump = 0.01f;
    [SerializeField] private float maxShake = 0.1f;
    [SerializeField] private float block = 0;
    [SerializeField] private float drift = 0;
    [SerializeField] private float jitter = 0;
    [SerializeField] private float jump = 0;
    [SerializeField] private float shake = 0;

    [SerializeField] private float blockShuffleRate = 60f;
    [SerializeField] private float flickerStrength = 1f;
    //[SerializeField] private float flickerIntensity = 1f;
    [SerializeField] private float scanlineStrength = 1f;
    [SerializeField] private bool manageValue = true;

    float _prevTime;
    float _jumpTime;
    private float _blockTime;
    private int _blockSeed1 = 71;
    private int _blockSeed2 = 113;
    private int _blockStride = 1;
    
    void Start()
    {
        glitchEffect.material = Instantiate(glitchMaterial);
        FlickerAndScanline(false);
        UpdateBounds();
        //ChannelNavigationManager.Instance.OnUpdateChannelLayout += UpdateBounds;
    }

    public void UpdateBounds()
    {
        Vector3[] corners = new Vector3[4];
        glitchEffect.rectTransform.GetWorldCorners(corners);
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = Camera.main.WorldToScreenPoint(corners[i]);
        }
        glitchEffect.material.SetVector("_MeshBound", 
            new Vector4(corners[0].x, corners[0].y, corners[2].x, corners[2].y));
    }

    
    void Update()
    {
        if(!GameManager.Instance.isStreaming)
        {
            return;
        }
        UpdateParameters();
    }



    public void SetGlitchIntensity(float value)
    {
        if(!manageValue)
        {
            return;
        }
        block = Mathf.Lerp(0, maxBlock, value);
        drift = Mathf.Lerp(0, maxDrift, value);
        jitter = Mathf.Lerp(0, maxJitter, value);
        jump = Mathf.Lerp(0, maxJump, value);
        shake = Mathf.Lerp(0, maxShake, value);
        blockShuffleRate = Mathf.Lerp(0, 60, value);
    }

    public void Flicker(bool show)
    {
        if(show)
        {
            glitchEffect.material.SetFloat("_NoiseStrength", flickerStrength);
        }
        else
        {
            glitchEffect.material.SetFloat("_NoiseStrength", 0);
        }
    }

    public void Scanline(bool show)
    {
        if(show)
        {
            glitchEffect.material.SetFloat("_ScanlineStrength", scanlineStrength);
        }
        else
        {
            glitchEffect.material.SetFloat("_ScanlineStrength", 0);
        }
    }

    public void FlickerAndScanline(bool show)
    {
        Flicker(show);
        Scanline(show);
    }



    public void UpdateParameters()
    {
        if(!manageValue)
        {
            return;
        }
        var time = Time.time;
        var delta = time - _prevTime;
        _jumpTime += delta * jump * 11.3f;

        _prevTime = time;

        var block3 = block*block*block;
    
        // Shuffle block parameters every 1/30 seconds.
        _blockTime += Time.deltaTime * blockShuffleRate;
        if (_blockTime > 1 && block > 0)
        {
            if (Random.value < 0.09f) _blockSeed1 += 251;
            if (Random.value < 0.29f) _blockSeed2 += 373;
            if (Random.value < 0.25f) _blockStride = Random.Range(1, 32);
            _blockTime = 0;
        }

        var vdrift = new Vector2(
            time * 606.11f % (Mathf.PI * 2),
            drift * 0.04f
        );

        // Jitter parameters (threshold, displacement)
        var jv = jitter;
        var vjitter = new Vector3(
            Mathf.Max(0, 1.001f - jv * 1.2f),
            0.002f + jv * jv * jv * 0.05f
        );

        var vjump = new Vector2(_jumpTime, jump);


        glitchEffect.material.SetFloat("_BlockStrength", block3);
        glitchEffect.material.SetInt("_BlockStride", _blockStride);
        glitchEffect.material.SetInt("_BlockSeed1", _blockSeed1);
        glitchEffect.material.SetInt("_BlockSeed2", _blockSeed2);
        glitchEffect.material.SetVector("_Drift", vdrift);
        glitchEffect.material.SetVector("_Jitter", vjitter);
        glitchEffect.material.SetVector("_Jump", vjump);
        glitchEffect.material.SetFloat("_Shake", shake);
        
        UpdateBounds();
    }

}
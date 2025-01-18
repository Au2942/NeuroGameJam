using UnityEngine;
using UnityEngine.UI;



public class GlitchOverlay : MonoBehaviour
{
    [SerializeField] private Image fsEffect;
    [SerializeField] private Image blockEffect;
    [SerializeField][Range(0,1)] private float block = 0;
    [SerializeField][Range(0,1)] private float drift = 0;
    [SerializeField][Range(0,1)] private float jitter = 0;
    [SerializeField][Range(0,1)] private float jump = 0;
    [SerializeField][Range(0,1)] private float shake = 0;

    [SerializeField] private float blockShuffleRate = 60f;
    [SerializeField] private float origFlicker = 50;
    [SerializeField] private float origScanline = 600;
    float _prevTime;
    float _jumpTime;
    private float _blockTime;
    private int _blockSeed1 = 71;
    private int _blockSeed2 = 113;
    private int _blockStride = 1;
    
    void Start()
    {
        FlickerAndScanline(false);
        blockEffect.materialForRendering.SetVector("_MeshSize", new Vector4(blockEffect.rectTransform.rect.width, blockEffect.rectTransform.rect.height, 0, 0));
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
        // block = value;
        // drift = value;
        // jitter = value;
        // jump = value;
        // shake = value;
        // blockShuffleRate = value*60f;
    }

    public void Flicker(bool show)
    {
        if(show)
        {
            fsEffect.materialForRendering.SetFloat("_NoiseAmount", origFlicker);
        }
        else
        {
            fsEffect.materialForRendering.SetFloat("_NoiseAmount", 0f);
        }
    }

    public void Scanline(bool show)
    {
        if(show)
        {
            fsEffect.materialForRendering.SetFloat("_ScanlineAmount", origScanline);
        }
        else
        {
            fsEffect.materialForRendering.SetFloat("_ScanlineAmount", 0f);
        }
    }

    public void FlickerAndScanline(bool show)
    {
        Flicker(show);
        Scanline(show);
    }



    public void UpdateParameters()
    {
        var time = Time.time;
        var delta = time - _prevTime;
        _jumpTime += delta * jump * 11.3f;
        _prevTime = time;

        var block3 = block;

        // Shuffle block parameters every 1/30 seconds.
        _blockTime += Time.deltaTime * blockShuffleRate;
        if (_blockTime > 1)
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


        blockEffect.materialForRendering.SetFloat("_BlockStrength", block3);
        blockEffect.materialForRendering.SetInt("_BlockStride", _blockStride);
        blockEffect.materialForRendering.SetInt("_BlockSeed1", _blockSeed1);
        blockEffect.materialForRendering.SetInt("_BlockSeed2", _blockSeed2);
        blockEffect.materialForRendering.SetVector("_Drift", vdrift);
        blockEffect.materialForRendering.SetVector("_Jitter", vjitter);
        blockEffect.materialForRendering.SetVector("_Jump", vjump);
        blockEffect.materialForRendering.SetFloat("_Shake", shake);

    }
}
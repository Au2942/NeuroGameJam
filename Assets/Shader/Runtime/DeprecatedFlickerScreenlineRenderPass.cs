/*
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class FlickerScreenlineRenderPass : ScriptableRenderPass
{
    private FlickerScreenlineSettings defaultSettings;
    private Material material;
    private RenderTextureDescriptor FlickerScreenlineTextureDescriptor;

    private static readonly int noiseStrengthID = Shader.PropertyToID("_NoiseStrength");
    private static readonly int noiseAmountID = Shader.PropertyToID("_NoiseAmount");
    private static readonly int noiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int scanlineStrengthID = Shader.PropertyToID("_ScanlineStrength");
    private static readonly int scanlineAmountID = Shader.PropertyToID("_ScanlineAmount");
    private const string k_FlickerScreenlineTextureName = "_FlickerScreenlineTextureName";
    private const string k_FlickerScreenlinePassName = "FlickerScanlinePass";



    public FlickerScreenlineRenderPass(Material material, FlickerScreenlineSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;

        FlickerScreenlineTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    { 
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();


        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Set the blur texture size to be the same as the camera target size.
        FlickerScreenlineTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
        FlickerScreenlineTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
        FlickerScreenlineTextureDescriptor.depthBufferBits = 0;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, FlickerScreenlineTextureDescriptor, k_FlickerScreenlineTextureName, false);
        
        // Update the blur settings in the material
        UpdateFlickerScreenlineSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;
        RenderGraphUtils.BlitMaterialParameters paraFlickerScreenline = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraFlickerScreenline, k_FlickerScreenlinePassName);
    }

    private void UpdateFlickerScreenlineSettings()
    {
        if (material == null) return;

        material.SetFloat(noiseStrengthID, defaultSettings.noiseStrength);
        material.SetFloat(noiseAmountID, defaultSettings.noiseAmount);
        material.SetFloat(noiseIntensityID, defaultSettings.noiseIntensity);
        material.SetFloat(scanlineStrengthID, defaultSettings.scanlineStrength);
        material.SetFloat(scanlineAmountID, defaultSettings.scanlineAmount);
    }
}   
*/
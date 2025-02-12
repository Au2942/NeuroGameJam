using UnityEngine;
using UnityEngine.UI;

public class ColorStripes : MonoBehaviour
{
    public Material ColorStripesMaterial;
    public Image BaseImage;
    public int Frequency = 100;
    private Material tempMaterial;
    //32 chunks each for a total of 32x4 = 128 chunks
    private int[] bitmasks = new int[4]; // Array to hold the bitmasks

    void Awake()
    {
        tempMaterial = new Material(ColorStripesMaterial);
        BaseImage.material = tempMaterial;
    }



    public void SetChunkRange(int startIndex, int endIndex, int value = 1, bool wrap = false)
    {
        if (startIndex > endIndex) (startIndex, endIndex) = (endIndex, startIndex); // Ensure correct order
        if (startIndex < 0) startIndex = 0;
        if (endIndex >= Frequency) endIndex = Frequency - 1;

        for (int i = startIndex; i <= endIndex; i++)
        {
            SetChunk(i, value, wrap, false);
        }

        UpdateShader();
    }

    public void SetChunk(int chunkIndex, int value = 1, bool wrap = false, bool update = true)
    {
        if(wrap)
        {
            chunkIndex = chunkIndex % Frequency;
        }
        else if (chunkIndex < 0 || chunkIndex >= Frequency)
        {
            Debug.LogWarning("Chunk index out of range: " + chunkIndex);
            return;
        }

        int groupIndex = chunkIndex / 32; // Which bitmask to modify (0-3)
        int bitIndex = chunkIndex % 32;   // Which bit within the bitmask
        int bitMask = 1 << bitIndex;      // Create a mask for the specific bit

        // Set or clear the bit depending on the value
        if (value == 0)
        {
            bitmasks[groupIndex] &= ~bitMask; // Clear the bit
        }
        else
        {
            bitmasks[groupIndex] |= bitMask;  // Set the bit
        }

        if(update) UpdateShader();
    }


    void UpdateShader()
    {
        tempMaterial.SetInteger("_Frequency", Frequency);
        tempMaterial.SetInteger("_Bitmask1", bitmasks[0]);
        tempMaterial.SetInteger("_Bitmask2", bitmasks[1]);
        tempMaterial.SetInteger("_Bitmask3", bitmasks[2]);
        tempMaterial.SetInteger("_Bitmask4", bitmasks[3]);
    }

    void OnDestroy()
    {
        if(Application.isPlaying)
        {
            Destroy(tempMaterial);
        }
        else
        {
            DestroyImmediate(tempMaterial,true);
        }
    }
}
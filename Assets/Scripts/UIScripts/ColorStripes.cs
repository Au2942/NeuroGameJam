using UnityEngine;
using UnityEngine.UI;

public class ColorStripes : MonoBehaviour
{
    public Material ColorStripesMaterial;
    public Image BaseImage;
    public int Frequency = 100;
    private Material tempMaterial;
    //32 chunks each for a total of 32x4 = 128 chunks
    private int[] bitmaskA = new int[4]; // Array to hold the lsb bitmasks
    private int[] bitmaskB = new int[4]; // Array to hold the msb bitmasks

    void Awake()
    {
        tempMaterial = new Material(ColorStripesMaterial);
        BaseImage.material = tempMaterial;
    }

    public void SetColor(int index, Color color)
    {
        switch(index)
        {
            case 0:
                tempMaterial.SetColor("_ColorA", color);
                break;
            case 1:
                tempMaterial.SetColor("_ColorB", color);
                break;
            case 2:
                tempMaterial.SetColor("_ColorC", color);
                break;
            case 3:
                tempMaterial.SetColor("_ColorD", color);
                break;
            default:
                Debug.LogWarning("Color index out of range: " + index);
                break;
        }
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
            chunkIndex %= Frequency;
        }
        else if (chunkIndex < 0 || chunkIndex >= Frequency)
        {
            Debug.LogWarning("Chunk index out of range: " + chunkIndex);
            return;
        }

        int groupIndex = chunkIndex / 32; 
        int bitIndex = chunkIndex % 32;   
        int bitMask = 1 << bitIndex;

        // Set two bits for 3 colors (00, 01, 10)
        switch (value)
        {
            case 0: // Color 1 (00)
                bitmaskA[groupIndex] &= ~bitMask; // Clear LSB
                bitmaskB[groupIndex] &= ~bitMask; // Clear MSB
                break;
            case 1: // Color 2 (01)
                bitmaskA[groupIndex] |= bitMask;  // Set LSB
                bitmaskB[groupIndex] &= ~bitMask; // Clear MSB
                break;
            case 2: // Color 3 (10)
                bitmaskA[groupIndex] &= ~bitMask; // Clear LSB
                bitmaskB[groupIndex] |= bitMask;  // Set MSB
                break;
            case 3: // Color 4 (11)
                bitmaskA[groupIndex] |= bitMask;  // Set LSB
                bitmaskB[groupIndex] |= bitMask;  // Set MSB
                break;
        }

        if (update) UpdateShader();
    }


    public void UpdateShader()
    {
        tempMaterial.SetInteger("_Frequency", Frequency);
        tempMaterial.SetInteger("_BitmaskA1", bitmaskA[0]);
        tempMaterial.SetInteger("_BitmaskA2", bitmaskA[1]);
        tempMaterial.SetInteger("_BitmaskA3", bitmaskA[2]);
        tempMaterial.SetInteger("_BitmaskA4", bitmaskA[3]);
        tempMaterial.SetInteger("_BitmaskB1", bitmaskB[0]);
        tempMaterial.SetInteger("_BitmaskB2", bitmaskB[1]);
        tempMaterial.SetInteger("_BitmaskB3", bitmaskB[2]);
        tempMaterial.SetInteger("_BitmaskB4", bitmaskB[3]);
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

    public void SetBitmasks(int[] values)
    {
        for (int i = 0; i < 4; i++)
        {
            bitmaskA[i] = values[i];
        }
        for (int i = 4; i < 8; i++)
        {
            bitmaskB[i - 4] = values[i];
        }
    }

    public void GetBitmasks(out int[] bitmaskA, out int[] bitmaskB)
    {
        bitmaskA = this.bitmaskA;
        bitmaskB = this.bitmaskB;
    }

    public int[] GetBitmaskA()
    {
        return bitmaskA;
    }

    public int[] GetBitmaskB()
    {
        return bitmaskB;
    }
}
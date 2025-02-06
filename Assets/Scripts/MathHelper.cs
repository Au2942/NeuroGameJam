using UnityEngine;
public static class MathHelper
{

    public static float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

}

public static class Easing
{
    public enum Ease
    {
        EaseInQuad,
        EaseOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInQuint,
        EaseOutQuint,
    }

    public static float EaseInQuad(float t)
    {
        return t * t;
    }

    public static float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    public static float EaseOutCubic(float t)
    {
        return 1 - (1 - t) * (1 - t) * (1 - t);
    }

    public static float EaseInQuart(float t)
    {
        return t * t * t * t;
    }

    public static float EaseOutQuart(float t)
    {
        return 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t);
    }

    public static float EaseInQuint(float t)
    {
        return t * t * t * t * t;
    }

    public static float EaseOutQuint(float t)
    {
        return 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t) * (1 - t);
    }
}



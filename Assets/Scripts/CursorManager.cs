using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    [SerializeField] CustomCursor defaultCursor;

    bool _overrided = false;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }else
            Instance = this;
    }
    void Start()
    {
        // Set the default cursor at the start
        SetDefaultCursor();
    }

    public IEnumerator SetCustomCursorForRealSecond(CustomCursor cursor, float time, bool force = false)
    {
        if(force) _overrided = true;
        SetCustomCursor(cursor);
        yield return new WaitForSecondsRealtime(time);
        SetDefaultCursor();
        _overrided = false;
    }

    public void SetCustomCursor(CustomCursor cursor)
    {
        if( _overrided || !Application.isFocused) return;
        Cursor.SetCursor(cursor.texture, cursor.hotspot, CursorMode.Auto);
    }
    public void SetDefaultCursor()
    {
        if(_overrided || !Application.isFocused) return;
        SetCustomCursor(defaultCursor);
    }

}

[System.Serializable]
public class CustomCursor
{
    [SerializeField] public Texture2D texture;
    [SerializeField] public Vector2 hotspot;
}


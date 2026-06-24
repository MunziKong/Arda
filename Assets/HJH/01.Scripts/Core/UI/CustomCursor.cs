using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}
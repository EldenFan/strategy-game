using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TileClickHandler : MonoBehaviour
{
    public Tilemap tilemap;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Vector3 worldPos =
                 Camera.main.ScreenToWorldPoint(mousePos);

            Vector3Int cellPos =
                tilemap.WorldToCell(worldPos);

            TileClicked?.Invoke(
                new Vector2Int(cellPos.x, cellPos.y));
        }
    }

    public event Action<Vector2Int> TileClicked;
}
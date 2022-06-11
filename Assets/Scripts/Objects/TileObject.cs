using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [Header("Тайловые данные")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;
    public virtual void Initialize(BoundsInt bounds)
    {
        Debug.Log($"{name} не определен");
        InitializeTilePlace(bounds);
    }

    private protected void InitializeTilePlace(BoundsInt bounds)
    {
        tilePlace = GetTilePosition(bounds);
        name += $"{tilePlace}";
    }

    public Vector2Int GetTilePosition(BoundsInt bounds) // от 0+
    {
        //позиция тайла из матрицы
        return new Vector2Int((int)(transform.position.x + Mathf.Abs(bounds.xMin)),
                              (int)(transform.position.y + Mathf.Abs(bounds.yMin)));
    }
}

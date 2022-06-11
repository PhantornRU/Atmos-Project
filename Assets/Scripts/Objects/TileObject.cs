using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [Header("Тайловые данные")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;
    public virtual void Initialize(Vector2Int _tilePlace)
    {
        Debug.Log($"{name} не определен");
        InitializeTilePlace(_tilePlace);
    }

    private protected void InitializeTilePlace(Vector2Int _tilePlace)
    {
        tilePlace = _tilePlace;
        name += $"{tilePlace}";
    }

    //public Vector2Int GetTilePosition() // от 0+
    //{
    //    //позиция тайла из матрицы
    //    return new Vector2Int((int)(transform.position.x + Mathf.Abs(tilesArray.bounds.xMin)),
    //                          (int)(transform.position.y + Mathf.Abs(tilesArray.bounds.yMin)));
    //}
}

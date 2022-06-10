using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [Header("�������� ������")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;
    public virtual void Initialize(Vector2Int _tilePlace)
    {
        Debug.Log($"{name} �� ���������");
        InitializeTilePlace(_tilePlace);
    }

    private protected void InitializeTilePlace(Vector2Int _tilePlace)
    {
        tilePlace = _tilePlace;
        name += $"{tilePlace}";
    }
}

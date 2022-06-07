using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [Header("Тайловые данные")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;
    public virtual void Initialize()
    {
        Debug.Log($"{name} не определен");
    }
}

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

    //public Vector2Int GetTilePosition() // �� 0+
    //{
    //    //������� ����� �� �������
    //    return new Vector2Int((int)(transform.position.x + Mathf.Abs(tilesArray.bounds.xMin)),
    //                          (int)(transform.position.y + Mathf.Abs(tilesArray.bounds.yMin)));
    //}
}

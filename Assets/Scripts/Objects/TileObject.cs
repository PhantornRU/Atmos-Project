using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [Header("�������� ������")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;
    public virtual void Initialize(BoundsInt bounds)
    {
        Debug.Log($"{name} �� ���������");
        InitializeTilePlace(bounds);
    }

    private protected void InitializeTilePlace(BoundsInt bounds)
    {
        tilePlace = GetTilePosition(bounds);
        name += $"{tilePlace}";
    }

    public Vector2Int GetTilePosition(BoundsInt bounds) // �� 0+
    {
        //������� ����� �� �������
        return new Vector2Int((int)(transform.position.x + Mathf.Abs(bounds.xMin)),
                              (int)(transform.position.y + Mathf.Abs(bounds.yMin)));
    }
}

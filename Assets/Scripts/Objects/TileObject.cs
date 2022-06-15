using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour, IDamageable<float>
{
    [Header("�������� ������")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;

    private Integrity integrity;

    public virtual void Initialize(BoundsInt bounds)
    {
        //Debug.Log($"{name} �� ���������");
        InitializeTilePlace(bounds);

        integrity = GetComponent<Integrity>();
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

    /// <summary>
    /// ��������� ����� ����������� �������
    /// </summary>
    /// <param name="damageTaken"></param>
    public void Damage(float damageTaken)
    {
        Debug.Log($"������ {name} ������� ����������� � ����������: {damageTaken}");

        integrity.ApplyDamage(damageTaken);
        if (integrity.integrity >= 0)
        {
            ActivateBeforeDestroyed();
        }
    }

    private void OnDestroy()
    {
        if (tilesArray != null)
        {
            ActivateBeforeDestroyed();
        }
    }

    /// <summary>
    /// ���������� �������� ����� ������������ �������
    /// </summary>
    public virtual void ActivateBeforeDestroyed()
    {
        //Debug.Log($"��������� ������ {name}");

        Destroy(this.gameObject);
    }
}

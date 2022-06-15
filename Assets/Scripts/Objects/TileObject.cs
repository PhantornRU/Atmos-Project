using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour, IDamageable<float>
{
    [Header("Тайловые данные")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;

    private Integrity integrity;

    public virtual void Initialize(BoundsInt bounds)
    {
        //Debug.Log($"{name} не определен");
        InitializeTilePlace(bounds);

        integrity = GetComponent<Integrity>();
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

    /// <summary>
    /// Нанесение урона целостности объекта
    /// </summary>
    /// <param name="damageTaken"></param>
    public void Damage(float damageTaken)
    {
        Debug.Log($"Объект {name} получит повреждение в количестве: {damageTaken}");

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
    /// Выполнение процедур перед уничтожением объекта
    /// </summary>
    public virtual void ActivateBeforeDestroyed()
    {
        //Debug.Log($"Уничтожен объект {name}");

        Destroy(this.gameObject);
    }
}

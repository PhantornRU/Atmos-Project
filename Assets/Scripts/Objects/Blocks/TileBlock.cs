using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[SelectionBase]
public class TileBlock : TileObject
{
    [Header("Тайловые данные")]
    public bool isBlockGas = true;

    [Tooltip("Спрайты меняющие текущий спрайт при нанесении урона")]
    public List<Sprite> spritesWall;

    /// <summary>
    /// Инициализация тайла блокиратора
    /// </summary>
    /// <param name="_tilesArray"></param>
    /// <param name="_tilePlace"></param>
    /// <param name="_name"></param>
    public void Initialize(TileMapArray _tilesArray, Vector2Int _tilePlace, string _name)
    {
        tilesArray = _tilesArray; //задаем ссылку на текущий массив
        tilePlace = _tilePlace; //индексы позиции

        name = $"{_name}{tilePlace}";

        //задаем параметры для тайлов газа где уже есть тайл
        if (tilesArray.tilesGas[tilePlace.x, tilePlace.y] != null)
        {   
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].ActivateBlockGas(); //блокиируем прохождение газа
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].isNeedSmoke = false; //блокируем создание дыма
        }
    }

    private void ChangeSprite()
    {

    }

    /// <summary>
    /// Деактивация блокировки газа
    /// </summary>
    public void DeactivateBlockGas()
    {
        isBlockGas = false;
        //запускаем обработку передачи давления на тайле, если тот присутствует
        if (tilesArray.tilesGas[tilePlace.x, tilePlace.y] != null)
        {
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].DeactivateBlockGas();
            //tilesArray.tilesGas[tilePlace.x, tilePlace.y].isActive = true;
        }
    }

    /// <summary> 
    /// Активация блокировки газа 
    /// </summary>
    public void ActivateBlockGas()
    {
        isBlockGas = true;
        if (tilesArray.tilesGas[tilePlace.x, tilePlace.y] != null)
        {
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].ActivateBlockGas();
        }
    }

    public override void ActivateBeforeDestroyed()
    {
        DeactivateBlockGas();
        base.ActivateBeforeDestroyed();
    }
}

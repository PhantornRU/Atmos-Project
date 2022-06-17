using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[SelectionBase]
public class TileBlock : TileObject
{
    [Header("Тайловые данные")]
    public bool isBlockGas = true;
    public GameObject objectWhenDisassembly;

    [HideInInspector] public bool isNeedToDestroy = false;
    [HideInInspector] public bool isNeedToComplete = false;

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
        ActivateBlockGas(); //блокиируем прохождение газа
    }

    AssemblyScript assemblyScript;

    public void Diassamble()
    {
        if (objectWhenDisassembly != null)
        {
            GameObject object_disassebmly = Instantiate(objectWhenDisassembly, transform);
            object_disassebmly.transform.parent = transform.parent;
            TileBlock tileBlock = object_disassebmly.GetComponent<TileBlock>();
            tileBlock.Initialize(tilesArray, tilePlace, name);

            tilesArray.tilesBlock[tilePlace.x, tilePlace.y] = tileBlock;

            //Debug.Log("Создан объект " + object_disassebmly.name +" для разбора от: " + name);
            isNeedToDestroy = true;
            //ActivateBeforeDestroyed();
        }
        else
        {
            //присваиваем текущий скрипт
            if (assemblyScript == null)
            {
                assemblyScript = GetComponent<AssemblyScript>();
            }
            else // если не пуст, то проводим операцию дальше
            {
                //Debug.Log("Меняем состояние у: " + name);
                assemblyScript.DisassemblyState();
            }
        }
    }

    public void Assamble()
    {
        //если скрипт равен null, то действий не производим
        if (assemblyScript == null)
        {
            //assemblyScript = GetComponent<AssemblyScript>();
            Debug.Log("Отсутствует ассембл, действие не произведено");
        }
        //если скрипт уже присвоен после разборки
        else
        {
            assemblyScript.AssemblyState();
        }
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

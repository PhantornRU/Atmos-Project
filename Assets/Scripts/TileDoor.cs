using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[SelectionBase]
public class TileDoor : TileBlock
{
    DoorAnimator doorAnimator;
    public bool isOpen = false;
    public float speed = 1;

    /// <summary>
    /// Инициализация основных компонентов для работы
    /// </summary>
    public void InitializeDoor(TileMapArray _tilesArray, Vector2Int _tilePlace, string _name)
    {
        Initialize(_tilesArray, _tilePlace, _name);

        doorAnimator = GetComponent<DoorAnimator>();
        doorAnimator.Initialize(speed);
    }

    /// <summary>
    /// Изменить состояние двери и закрыть или открыть её
    /// </summary>
    public void ChangeState()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    void Open()
    {
        DeactivateBlockGas();
        doorAnimator.Animate(isOpen, speed);
    }

    void Close()
    {
        ActivateBlockGas();
        doorAnimator.Animate(isOpen, speed);
    }
}

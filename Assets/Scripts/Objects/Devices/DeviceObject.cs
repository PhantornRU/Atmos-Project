using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceObject : TileObject
{
    public bool isToggleOn = false;

    /// <summary>
    /// Переключить девайс
    /// </summary>
    public void ToggleDevice()
    {
        isToggleOn = !isToggleOn;

        if (isToggleOn)
        {
            ToggleOn();
        }
        else
        {
            ToggleOff();
        }
    }

    /// <summary>
    /// Включить девайс
    /// </summary>
    public virtual void ToggleOn()
    {
        Debug.Log($"{name}-переключатель [{isToggleOn}] не определен");
    }

    /// <summary>
    /// Выключить девай
    /// </summary>
    public virtual void ToggleOff()
    {
        Debug.Log($"{name}-переключатель [{isToggleOn}] не определен");
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DevicesManager : MonoBehaviour
{
    //Девайс-менеджер не нужен, так как каждый девайс можно инициализировать методом Start/Awake(Уточнить за что эти методы отвечают)
    //А после прикрутить им возможность воздействия игрока на них.

    public List<Tilemap> atmosphereTileMapDevices; //список всех тайлмап объектов-девайсов атмосферики
    public List<Tilemap> usingTileMapDevices; //список всех тайлмап объектов-девайсов с которыми можно взаимодействовать

    private List<AtmosDevice> listAtmosDevices = new List<AtmosDevice>();

    //private bool isInitializeCompleted = false; //проверка на завершение инициализации

    public void Initialize(float _tick_time, BoundsInt bounds)
    {
        DevicesInitialize(bounds);

        //isInitializeCompleted = true;

    }

    private void DevicesInitialize(BoundsInt bounds)
    {
        //инициализируем все девайсы с которыми можно взаимодействовать
        foreach (Tilemap tilemap_devices in usingTileMapDevices)
        {
            foreach (Transform devices in tilemap_devices.transform)
            {
                devices.GetComponent<DeviceObject>().Initialize(GetTilePosition(devices, bounds));
            }
        }

        //инициализируем все девайсы атмосфферики
        foreach (Tilemap tilemap_devices in atmosphereTileMapDevices)
        {
            foreach (Transform devices in tilemap_devices.transform)
            {
                devices.GetComponent<AtmosDevice>().Initialize(GetTilePosition(devices, bounds));
                listAtmosDevices.Add(devices.GetComponent<AtmosDevice>());
            }
        }
    }

    private Vector2Int GetTilePosition(Transform c_object, BoundsInt bounds)
    {
        //позиция тайла из матрицы
        return new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
                              (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));
    }

    /// <summary>
    /// Обновление массива тайлов по границам
    /// </summary>
    public void UpdateDevices()
    {
        //обновляем атмосферные девайсы
        foreach (Tilemap tilemap_devices in atmosphereTileMapDevices)
        {
            foreach (AtmosDevice devices in listAtmosDevices)
            {
                //Debug.Log("Обновлен девайс: " + devices.name);
                devices.UpdateAtmosDevice();
            }
        }
    }
}

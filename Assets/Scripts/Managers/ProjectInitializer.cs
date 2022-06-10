using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectInitializer : MonoBehaviour
{
    public float tick_time = 0.1f; //Время в секундах для повтора
    [SerializeField] float tick_curr_time; //Текущее время

    private List<TileMapArray> tileMapArray;
    private List<DevicesManager> devicesManager;

    void Start()
    {
        tileMapArray = new List<TileMapArray>();
        devicesManager = new List<DevicesManager>();

        //границы для девайс менеджера
        BoundsInt bounds = new BoundsInt();

        //Инициализация объектов по определенному порядку для их корректной обработки

        //Инициализируем тайлмапы для их внутренней обработки
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            tileMap.Initialize(tick_time);
            tileMapArray.Add(tileMap);

            bounds = tileMap.bounds; //!!! временно, если не будет добавляться дополнительные тайлмапы !!!
        }

        //Инициализируем девайсы
        foreach (DevicesManager devices in FindObjectsOfType<DevicesManager>())
        {
            devices.Initialize(tick_time, bounds);
            devicesManager.Add(devices);
        }

        //Инициализируем игрока и его компоненты после тайл мапов
        FindObjectOfType<PlayerController>().Initialize();
    }

    public bool isNeedUpdateArray = true;//= false; !!!вставлена временная заглушка!!! //проверка, необходимо ли обновление или можно не выполнять метод UpdateArray()

    //Обновляем менеджеры
    void FixedUpdate()
    {
        tick_curr_time -= Time.deltaTime; // Вычитаем время кадра
        if (tick_curr_time <= 0) //Время вышло
        {

            foreach (TileMapArray tileMap in tileMapArray)
            {
                tileMap.UpdateArray();
                //isNeedUpdateArray = tileMap.countActivate > 0; //isNeedUpdateArray = tileMap.countActivate > 0 ? true : false;
            }

            //Обновляем девайсы
            if (isNeedUpdateArray)
            {
                foreach (DevicesManager devices in devicesManager)
                {
                    devices.UpdateDevices();
                }
            }

            tick_curr_time = tick_time; // повторный запуск таймера
            //isNeedUpdateArray = true;
        }

    }
}

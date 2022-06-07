using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectInitializer : MonoBehaviour
{
    public float tick_time = 0.1f; //Время в секундах для повтора

    void Start()
    {
        //Инициализация объектов по определенному порядку для их корректной обработки

        //Инициализируем тайл мапы для их внутренней обработки
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            tileMap.Initialize(tick_time);
        }

        //Инициализируем игрока и его компоненты после тайл мапов
        FindObjectOfType<PlayerController>().Initialize();
    }
}

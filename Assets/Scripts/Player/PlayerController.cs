using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IDamageable<float>
{
    [Header("Параметры игрока")]
    [Min(0)] public int health = 100;
    [Min(0)] public int healthMax = 100;
    // менеджеры и скрипты
    ProjectInitializer projectInitializer;
    [HideInInspector] public TileMapArray tilesArray;

    //внутренные используемые переменные
    Camera mainCamera;

    // параметры и скрипты игрока
    PlayerMovement playerMovement;
    PlayerInterfaceScript playerInterface;
    PlayerBuilderScript playerBuilder;
    bool isInitialized = false;

    public void Initialize()
    {
        mainCamera = Camera.main;

        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.Initialize();
        playerInterface = GetComponent<PlayerInterfaceScript>();
        playerBuilder = GetComponent<PlayerBuilderScript>();
        playerBuilder.Initialize(tilesArray);

        isInitialized = true;

        playerInterface.ChangeHealthUI(health, healthMax);
    }

    private void Start()
    {
        //ищем по координатам и проверяем находится ли объект в радиусе границ одного из доступных тайлЭррей, на случай если этих зон будет несколько
        tilesArray = FindObjectOfType<TileMapArray>().GetComponent<TileMapArray>(); //!!!!!!временная заглушка!!!!!!!
        projectInitializer = FindObjectOfType<ProjectInitializer>().GetComponent<ProjectInitializer>(); //!!!!!!временная заглушка!!!!!!!
    }

    float tick_curr_time; //Текущее время

    private void Update()
    {
        if (isInitialized)
        {
            //Методы нажатия на ЛКМ и ПКМ
            tick_curr_time -= Time.deltaTime; // Вычитаем время кадра
            if (tick_curr_time <= 0)
            {
                tick_curr_time = ProjectInitializer.tick_time;
                playerBuilder.ButtonsFunctions();
            }
        }
    }

    public void Damage(float damageTaken)
    {
        Debug.Log($"Объект {name} получит повреждение в количестве: {damageTaken}");

        health = Mathf.Clamp(health - (int)damageTaken, 0, healthMax);

        playerInterface.ChangeHealthUI(health, healthMax);
    }
}

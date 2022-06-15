using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    // менеджеры и скрипты
    ProjectInitializer projectInitializer;
    TileMapArray tilesArray;

    //внутренные используемые переменные
    Camera mainCamera;

    // параметры и скрипты игрока
    PlayerMovement playerMovement;
    bool isInitialized = false;

    [Header("Тестовые объекты")]
    public RuleTile ruleTileLatice;
    public RuleTile ruleTileWall;

    LeftClickMode LCMode = LeftClickMode.None;

    /// <summary>
    /// Режимы доступные по нажатию на левую кнопку мыши
    /// </summary>
    private enum LeftClickMode
    {
        None,
        Interact,
        Damage,
        Create,
        AddGas
    }

    public void Initialize()
    {
        mainCamera = Camera.main;

        CheckTileArray();

        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.Initialize();

        isInitialized = true;
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
                ButtonsFunctions();
            }
        }
    }

    void CheckTileArray()
    {
        //ведем поиск для каждого тайл мапа
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            Debug.Log($"Найден тайлмап {tileMap.name}, его данные: {tileMap.bounds}, max: {tileMap.bounds.max}, min: {tileMap.bounds.min}, размер {tileMap.bounds.size}");

            if (transform.position.x >= tileMap.bounds.xMin && transform.position.x <= tileMap.bounds.xMax &&
                transform.position.y >= tileMap.bounds.yMin && transform.position.y <= tileMap.bounds.yMax)
            {
                tilesArray = tileMap;
                Debug.Log($"Объект {name} находится на {tileMap.name}");
            }
        }

    }

    /// <summary>
    /// Вызываемые функции по нажатию мыши. Для тестов.
    /// </summary>
    private void ButtonsFunctions()
    {
        //тест
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (TileDoor door in tilesArray.tilesDoor)
            {
                if (door != null)
                {
                    door.ChangeState();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            CheckTileArray();
        }

        //лкм
        if (Input.GetMouseButton(0))
        {
            //получение координат и позиции
            Vector3 clickWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickTilePosition = new Vector3Int((int)(tilesArray.transform.position.x + clickWorldPosition.x + Mathf.Abs(tilesArray.bounds.xMin)), 
                                                          (int)(tilesArray.transform.position.y + clickWorldPosition.y + Mathf.Abs(tilesArray.bounds.yMin)), 0);
            Vector3Int clickCellPosition = tilesArray.map[0].WorldToCell(Input.mousePosition);
            Vector3Int clickTileSetPosition = new Vector3Int((int)(tilesArray.transform.position.x + clickTilePosition.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                             (int)(tilesArray.transform.position.y + clickTilePosition.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);

            if (CheckInArrayBounds(clickTileSetPosition))
            {
                switch (LCMode)
                {
                    case LeftClickMode.None:
                        {
                            Debug.Log($"Отсутствует режим на ЛКМ: {LCMode}");
                            break;
                        }
                    case LeftClickMode.Interact:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Взаимодействие с тайлом [{clickTilePosition.x}, {clickTilePosition.y}]");

                            if (tilesArray.tilesDoor[clickTilePosition.x, clickTilePosition.y] != null)
                                try
                                {
                                    tilesArray.tilesDoor[clickTilePosition.x, clickTilePosition.y].ChangeState();
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"Выход за массив на тайле при обновлении {clickTilePosition}\nОшибка: {e}");
                                }

                            break;
                        }
                    case LeftClickMode.Create:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Создание по тайлу [{clickTilePosition.x}, {clickTilePosition.y}]");
                            //Debug.Log($"Создание тестового тайла TileSet: [{clickTileSetPosition.x}, {clickTileSetPosition.y}], Tile: [{clickTilePosition.x}, {clickTilePosition.y}], World: [{clickWorldPosition.x}, {clickCellPosition.y}], cell: [{clickCellPosition.x}, {clickWorldPosition.y}]");

                            //устанавливаем тайл
                            if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] == null)
                            {
                                int c_map = 0;
                                tilesArray.map[c_map].SetTile(clickTileSetPosition, ruleTileLatice);
                                tilesArray.TileAdd(clickTilePosition, c_map);


                            }
                            else if (tilesArray.tilesBlock[clickTilePosition.x, clickTilePosition.y] == null)
                            {
                                int c_map = 1;
                                tilesArray.map[c_map].SetTile(clickTileSetPosition, ruleTileWall);
                                tilesArray.TileAdd(clickTilePosition, c_map);

                                tilesArray.map[c_map].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //Обновляем тайлмап коллайдер

                                //отключаем дым под тайлом если он есть
                                if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.activeInHierarchy == true)
                                {
                                    tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.SetActive(false);
                                }
                            }
                            break;
                        }
                    case LeftClickMode.Damage:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Нанесен урон по тайлу [{clickTilePosition.x}, {clickTilePosition.y}]");

                            //убираем тайл
                            if (tilesArray.tilesBlock[clickTilePosition.x, clickTilePosition.y] != null)
                            {
                                tilesArray.map[1].SetTile(clickTileSetPosition, null); //убирание тайла стены и его GameObject
                                tilesArray.map[1].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //Обновляем тайлмап коллайдер

                                //включаем дым под тайлом если он есть
                                if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.activeInHierarchy == false)
                                {
                                    tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.SetActive(true);
                                }
                            }
                            else if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null)
                            {
                                //tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].UpdatePressure(0); //обновляем чтобы окружающие тайлы обновились
                                //tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].PressureTransmission(tilesArray.tick_time);
                                tilesArray.map[0].SetTile(clickTileSetPosition, null); //убирание тайла пола и его GameObject
                            }


                            break;
                        }
                    case LeftClickMode.AddGas:
                        {
                            if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null)
                                try
                                {
                                    Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \nДобавляем газ по тайлу: {tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].name}");
                                    tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].UpdateGas(500f, 0f); //добавляем давление
                                                                                                                         //tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].DeactivateBlockGas(); //деактивируем блок
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"Выход за массив на тайле при обновлении {clickTilePosition}\nОшибка: {e}");
                                }


                            break;
                        }
                    default:
                        {
                            Debug.Log($"Отсутствует режим на ЛКМ");
                            break;
                        }
                }
            }
        }

        //пкм
        if (Input.GetMouseButtonDown(1))
        {
            //получаем позиции
            Vector3 clickWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickTilePosition = new Vector3Int((int)(tilesArray.transform.position.x + clickWorldPosition.x + Mathf.Abs(tilesArray.bounds.xMin)),
                                                          (int)(tilesArray.transform.position.y + clickWorldPosition.y + Mathf.Abs(tilesArray.bounds.yMin)), 0);
            Vector3Int clickCellPosition = tilesArray.map[0].WorldToCell(Input.mousePosition);
            Vector3Int clickTileSetPosition = new Vector3Int((int)(tilesArray.transform.position.x + clickTilePosition.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                             (int)(tilesArray.transform.position.y + clickTilePosition.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);

            //информация о тайле
            if (CheckInArrayBounds(clickTileSetPosition))
            {
                string strPosition = $"WorldPosition: {clickWorldPosition}, CellPosition: {clickCellPosition}";
                string strGas = tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null ? $"и {tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y]} {tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].tilePlace}" : "тайла нет";
                string strPressure = tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null ? $"{tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].pressure}" : "давления нет";
                Debug.Log($"Tile {clickTilePosition}, давление = {strPressure}, находящиеся тайлы: {strGas} \n{strPosition}");
            }
        }
    }

    //Модя для включения на кнопках
    public void DeleteMode()
    {
        LCMode = LeftClickMode.Damage;
        Debug.Log($"Текущий режим левой кнопки мыши игрока: {LCMode}");
    }
    public void CreateMode()
    {
        LCMode = LeftClickMode.Create;
        Debug.Log($"Текущий режим левой кнопки мыши игрока: {LCMode}");
    }
    public void InteractMode()
    {
        LCMode = LeftClickMode.Interact;
        Debug.Log($"Текущий режим левой кнопки мыши игрока: {LCMode}");
    }
    public void AddGasMode()
    {
        LCMode = LeftClickMode.AddGas;
        Debug.Log($"Текущий режим левой кнопки мыши игрока: {LCMode}");
    }
    public void DefaultMode()
    {
        LCMode = LeftClickMode.None;
        Debug.Log($"Текущий режим левой кнопки мыши игрока: {LCMode}");
    }

    private bool CheckInArrayBounds(Vector3Int position)
    {
        if ((position.x >= tilesArray.bounds.xMax)
            || (position.y >= tilesArray.bounds.yMax)
            || (position.x <= tilesArray.bounds.xMin)
            || (position.y <= tilesArray.bounds.yMin)
            )
        {
            return false;
        }
        else return true;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
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
    bool isInitialized = false;

    [Header("Объекты для создания")]
    public RuleTile ruleTileLatice;
    public RuleTile ruleTileWall;

    public LeftClickMode LCMode = LeftClickMode.None;

    /// <summary>
    /// Режимы доступные по нажатию на левую кнопку мыши
    /// </summary>
    public enum LeftClickMode
    {
        None,
        Interact,
        Disassembly,
        Assembly,
        DebugCreate,
        DebugDelete,
        DebugAddGas
    }

    public void Initialize()
    {
        mainCamera = Camera.main;

        CheckTileArray();

        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.Initialize();

        playerInterface = GetComponent<PlayerInterfaceScript>();

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
            Vector3Int clickTilePlacePosition = new Vector3Int((int)(tilesArray.transform.position.x + clickWorldPosition.x + Mathf.Abs(tilesArray.bounds.xMin)), 
                                                          (int)(tilesArray.transform.position.y + clickWorldPosition.y + Mathf.Abs(tilesArray.bounds.yMin)), 0);
            Vector3Int clickCellPosition = tilesArray.map[(int)TileMapArray.TileMapType.blocks].WorldToCell(Input.mousePosition);
            Vector3Int clickTileArrayPosition = new Vector3Int((int)(tilesArray.transform.position.x + clickTilePlacePosition.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                             (int)(tilesArray.transform.position.y + clickTilePlacePosition.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);

            if (CheckInArrayBounds(clickTileArrayPosition))
            {
                switch (LCMode)
                {
                    case LeftClickMode.None:
                        {
                            Debug.Log($"Отсутствует режим на ЛКМ: {LCMode}");
                            //tilesArray.TileRemove(clickTileArrayPosition, (int)TileMapArray.TileMapType.blocks);
                            break;
                        }
                    case LeftClickMode.Interact:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Взаимодействие с тайлом [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                                try
                                {
                                    tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].ChangeState();
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"Выход за массив на тайле при обновлении {clickTilePlacePosition}\nОшибка: {e}");
                                }
                            break;
                        }
                    case LeftClickMode.DebugCreate:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Создание по тайлу [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //устанавливаем тайл
                            if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.floor;
                                tilesArray.map[c_map].SetTile(clickTileArrayPosition, ruleTileLatice);
                                tilesArray.TileAdd(clickTilePlacePosition, c_map);
                            }
                            else if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                            {
                                CreateWall(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                            }
                            break;
                        }
                    case LeftClickMode.DebugDelete:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Удаление по тайлу [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //разбираем тайл
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.blocks;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            } //или убираем его
                            else if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.floor;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            }
                            break;
                        }
                    case LeftClickMode.Disassembly:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Разобран тайл [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //разбираем тайл
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].Diassamble();
                                if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToDestroy)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.blocks;
                                    tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].ActivateBeforeDestroyed();
                                    tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);

                                    tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].CreateSmoke();
                                    tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].CreateText();
                                }
                            } //или убираем его
                            else if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.floor;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            }
                            break;
                        }
                    case LeftClickMode.Assembly:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Собран тайл [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //собираем тайл
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].Assamble();
                                if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToComplete)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.blocks;
                                    Debug.Log($"Создания тайла по: [clickTilePlacePosition:{clickTilePlacePosition}, clickTileArrayPosition:{clickTileArrayPosition}]");
                                    Destroy(tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].gameObject);
                                    CreateWall(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                                }
                            }

                            //повторно собираем тайл завершающим нажатием, занося его в массив и задавая ему необходимые данные, иначе сбор невозможен
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                            {
                                CreateWall(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                            }
                            break;
                        }
                    case LeftClickMode.DebugAddGas:
                        {
                            if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                                try
                                {
                                    Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \nДобавляем газ по тайлу: {tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].name}");
                                    tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].UpdateGas(500f, 0f); //добавляем давление
                                                                                                                         //tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].DeactivateBlockGas(); //деактивируем блок
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"Выход за массив на тайле при обновлении {clickTilePlacePosition}\nОшибка: {e}");
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

    private void CreateWall(Vector3Int clickTilePlacePosition, Vector3Int clickTileArrayPosition, RuleTile ruleTileWall)
    {
        int c_map = (int)TileMapArray.TileMapType.blocks;
        tilesArray.map[c_map].SetTile(clickTileArrayPosition, ruleTileWall);
        tilesArray.TileAdd(clickTilePlacePosition, c_map);
        tilesArray.map[c_map].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //Обновляем тайлмап коллайдер

        //отключаем дым под тайлом если он есть
        if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.activeInHierarchy == true)
        {
            tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.SetActive(false);
        }
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

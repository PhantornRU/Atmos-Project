using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerBuilderScript : MonoBehaviour
{
    [Header("Объекты для создания")]
    public RuleTile ruleTileLatice;
    public RuleTile ruleTileWall;
    public RuleTile ruleTileWindow;
    public GameObject gameObjectDoor;

    [Header("Текущие режимы и типы")]
    public LeftClickMode LCMode = LeftClickMode.None;
    public BuildType current_build_type = BuildType.None;
    TileBlock.BuildingType curBuildType = TileBlock.BuildingType.None;
    public bool isDebugMode = true;

    //внутренные используемые переменные
    Camera mainCamera;
    float radiusCheckPlayerMouse = 100f;
    [HideInInspector] public TileMapArray tilesArray;
    PlayerController playerController;
    PlayerInterfaceScript playerInterface;

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

    public enum BuildType
    {
        None,
        Wall,
        Window,
        Door
    }

    public void Initialize(TileMapArray _tilesArray)
    {
        mainCamera = Camera.main;
        tilesArray = _tilesArray;

        playerController = GetComponent<PlayerController>();
        playerInterface = GetComponent<PlayerInterfaceScript>();

        CheckTileArray();
    }

    /// <summary>
    /// Вызываемые функции по нажатию мыши.
    /// </summary>
    public void ButtonsFunctions()
    {
        //тест
        if (Input.GetKey(KeyCode.Space))
        {
            //открытие всех дверей
            foreach (TileDoor door in tilesArray.tilesDoor)
            {
                if (door != null)
                {
                    door.ChangeState();
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            CheckTileArray();
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            playerInterface.AssemblyMode();
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            playerInterface.DisassemblyMode();
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            playerInterface.InteractMode();
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

            if ((CheckMouseInRadius(radiusCheckPlayerMouse) || isDebugMode) && CheckInArrayBounds(clickTileArrayPosition))
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

                            if (current_build_type != BuildType.None)
                            {
                                //устанавливаем тайл
                                if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.floor;
                                    tilesArray.map[c_map].SetTile(clickTileArrayPosition, ruleTileLatice);
                                    tilesArray.TileAdd(clickTilePlacePosition, c_map);
                                }
                                else if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                                {
                                    switch (current_build_type)
                                    {
                                        case (BuildType.Wall):
                                            {
                                                CreateBlock(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                                                break;
                                            }
                                        case (BuildType.Window):
                                            {
                                                CreateBlock(clickTilePlacePosition, clickTileArrayPosition, ruleTileWindow);
                                                break;
                                            }
                                        case (BuildType.Door):
                                            {
                                                CreateDoor(clickTilePlacePosition, clickTileArrayPosition, gameObjectDoor);
                                                break;
                                            }
                                    }
                                }
                            }
                            break;
                        }
                    case LeftClickMode.DebugDelete:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Удаление по тайлу [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //разбираем тайл
                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.doors;
                                //Debug.Log($"Найдена дверь {tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].name} по координатам: [clickTilePlacePosition: {clickTilePlacePosition}, clickTileArrayPosition{clickTileArrayPosition}]");
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            }
                            else if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.blocks;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            } //или убираем пол
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
                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].Diassamble();
                                if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToDestroy)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.doors;
                                    tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].ActivateBeforeDestroyed();
                                    tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);

                                    tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].CreateSmoke();
                                    tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].CreateText();
                                }
                            }
                            else if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
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
                            }
                            ////или убираем пол
                            //else if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            //{
                            //    int c_map = (int)TileMapArray.TileMapType.floor;
                            //    tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            //}
                            break;
                        }
                    case LeftClickMode.Assembly:
                        {
                            Debug.Log($"Произведено действие игрока на ЛКМ: {LCMode} \n Собран тайл [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //собираем тайл

                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].Assamble();
                                if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToComplete)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.doors;
                                    Debug.Log($"Создания тайла по: [clickTilePlacePosition:{clickTilePlacePosition}, clickTileArrayPosition:{clickTileArrayPosition}]");
                                    Destroy(tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].gameObject);
                                    CreateDoor(clickTilePlacePosition, clickTileArrayPosition, gameObjectDoor);
                                }
                            }
                            else if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].Assamble();
                                if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToComplete)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.blocks;
                                    Debug.Log($"Создания тайла по: [clickTilePlacePosition:{clickTilePlacePosition}, clickTileArrayPosition:{clickTileArrayPosition}]");
                                    curBuildType = tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].current_building_type;
                                    Destroy(tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].gameObject);
                                    switch (curBuildType)
                                    {
                                        case (TileBlock.BuildingType.Wall):
                                            {
                                                CreateBlock(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                                                break;
                                            }
                                        case (TileBlock.BuildingType.Window):
                                            {
                                                CreateBlock(clickTilePlacePosition, clickTileArrayPosition, ruleTileWindow);
                                                break;
                                            }
                                        case (TileBlock.BuildingType.Door):
                                            {
                                                CreateDoor(clickTilePlacePosition, clickTileArrayPosition, gameObjectDoor);
                                                curBuildType = TileBlock.BuildingType.None;
                                                break;
                                            }
                                    }
                                }
                            }


                            //повторно собираем тайл завершающим нажатием, занося его в массив и задавая ему необходимые данные, иначе сбор невозможен
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                            {
                                switch (curBuildType)
                                {
                                    case (TileBlock.BuildingType.Wall):
                                        {
                                            CreateBlock(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                                            curBuildType = TileBlock.BuildingType.None;
                                            break;
                                        }
                                    case (TileBlock.BuildingType.Window):
                                        {
                                            CreateBlock(clickTilePlacePosition, clickTileArrayPosition, ruleTileWindow);
                                            curBuildType = TileBlock.BuildingType.None;
                                            break;
                                        }
                                        //case (TileBlock.BuildingType.Door):
                                        //    {
                                        //        CreateDoor(clickTilePlacePosition, clickTileArrayPosition, gameObjectDoor);
                                        //        break;
                                        //    }
                                }
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
    /// <summary>
    /// Создаем тайл по ruleTile и заносим его в массив
    /// </summary>
    /// <param name="clickTilePlacePosition"></param>
    /// <param name="clickTileArrayPosition"></param>
    /// <param name="ruleTile"></param>
    private void CreateBlock(Vector3Int clickTilePlacePosition, Vector3Int clickTileArrayPosition, RuleTile ruleTile)
    {
        int c_map = (int)TileMapArray.TileMapType.blocks;
        tilesArray.map[c_map].SetTile(clickTileArrayPosition, ruleTile);
        tilesArray.TileAdd(clickTilePlacePosition, c_map);
        tilesArray.map[c_map].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //Обновляем тайлмап коллайдер

        //tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].Initialize(tilesArray, (Vector2Int)clickTilePlacePosition, "TileBlockCreate");

        //отключаем дым под тайлом если он есть
        if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.activeInHierarchy == true)
        {
            tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.SetActive(false);
        }
    }

    /// <summary>
    /// Создаем объект двери и заносим его в массив
    /// </summary>
    /// <param name="clickTilePlacePosition"></param>
    /// <param name="clickTileArrayPosition"></param>
    /// <param name="_gameObjectDoor"></param>
    private void CreateDoor(Vector3Int clickTilePlacePosition, Vector3Int clickTileArrayPosition, GameObject _gameObjectDoor)
    {
        int c_map = (int)TileMapArray.TileMapType.doors;

        GameObject doorObject = Instantiate(_gameObjectDoor);
        doorObject.transform.SetParent(tilesArray.map[c_map].transform);
        doorObject.transform.position = tilesArray.transform.position + clickTileArrayPosition + new Vector3(0.5f, 0.5f, 0);
        tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] = doorObject.GetComponent<TileDoor>();

        tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].InitializeDoor(tilesArray, (Vector2Int)clickTilePlacePosition, "TileDoor");

        //tilesArray.TileAdd(clickTilePlacePosition, c_map);
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

    /// <summary>
    /// Проверка нахождение мыши в заданном радиусе в пикселях от центра экрана
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    private bool CheckMouseInRadius(float radius)
    {
        var mousePosition = Input.mousePosition;
        mousePosition.x -= Screen.width / 2;
        mousePosition.y -= Screen.height / 2;
        return Mathf.Abs(mousePosition.x) <= radius && Mathf.Abs(mousePosition.y) <= radius;
    }

    /// <summary>
    /// Нахождение тайлмапа на котором находится персонаж
    /// </summary>
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
}

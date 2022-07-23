using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

//В принципе можно создать матрицу где и буду храниться все данные и заменять тайлы через тайл мап,
//вместо создания гейм обжектов через палитру

//Grid Map
[DisallowMultipleComponent]
public class TileMapArray : MonoBehaviour
{
    [Header("Массивы")]
    public Tilemap[] map; //массив всех слоев карт

    //доступы к тайлмапам
    public TileGas[,] tilesGas; //массив газовых тайлов
    public TileBlock[,] tilesBlock; //массив блоковых тайлов - стены, стекло
    public TileDoor[,] tilesDoor; //массив объектов тайлов - двери
    public Tile[,] tilesPipe; //массив объектов тайлов - трубы, их девайсы
    public List<TilePipeNetwork> pipesNetwork; //список систем массива труб, внутри которых находятся входы труб

    HashSet<GameObject> hashObjects;

    public BoundsInt bounds; //границы массива и тайлов

    Camera mainCamera;

    [Header("Рассчетные переменные")]
    private float tick_time; //Время в секундах для повтора
    public int countActivate; //Счет невозможных тайлов для обновление

    //Типы ТайлМапов по нумерациям !!!лучше переделать с определением типа из получаемого списка!!!
    public enum TileMapType
    {
        floor,  //0
        blocks, //1 walls & windows
        doors,  //2
        devices   //3

    }

    private bool isInitializeCompleted = false; //проверка на завершение инициализации

    public void Initialize(float _tick_time)
    {
        tick_time = _tick_time;
        mainCamera = Camera.main;

        //заполняем наш массив полученными данными
        TilesArrayCreate();

        //Визуализируем данные на сцене
        TilesArrayInitializeCreateVisualization();

        hashObjects = new HashSet<GameObject>();

        isInitializeCompleted = true;
    }

    [HideInInspector] public bool isUpdateVisual = true;

    public void ToggleVisual()
    {
        isUpdateVisual = !isUpdateVisual;

        //переключаем все дымы и текста у газов
        for (int i = 0; i < tilesGas.Length; i++)
        {
            for (int j = 0; j < tilesGas.Length; j++)
            {
                try
                {
                    if (tilesGas[i, j] != null)
                    {
                        if (tilesGas[i, j].smokeObject.activeInHierarchy && tilesGas[i, j].textObject.isActiveAndEnabled)
                        {
                            tilesGas[i, j].smokeObject.SetActive(false);
                            tilesGas[i, j].textObject.gameObject.SetActive(false);
                        }
                        else
                        {
                            tilesGas[i, j].smokeObject.SetActive(true);
                            tilesGas[i, j].textObject.gameObject.SetActive(true);
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }


    /// <summary>
    /// Обновление массива тайлов по границам
    /// </summary>
    public void UpdateTileMaps()
    {
        //Запуск обновления тайлов газа
        countActivate = 0;
        for (int i = 0; i < tilesGas.Length; i++)
        {
            for (int j = 0; j < tilesGas.Length; j++)
            {
                try
                {
                    if (tilesGas[i, j] != null)
                    {
                        tilesGas[i, j].TransmissionGas(tick_time, isUpdateVisual);

                        if (tilesGas[i, j].isActive)
                        {
                            countActivate++;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        //обновляем системы труб с передачей газов
        foreach (TilePipeNetwork network in pipesNetwork)
        {
            network.UpdatePipeNetwork();
        }
    }

    /// <summary>
    /// Устанавливаем границы по всем объектам тайлмапа
    /// </summary>
    private void TilesBoundsCreate()
    {
        //установление границ
        map[(int)TileMapType.floor].CompressBounds();
        bounds = map[(int)TileMapType.floor].cellBounds;
        foreach (Tilemap c_map in map)
        {
            if (c_map != map[(int)TileMapType.floor])
            {
                c_map.CompressBounds();
                if (bounds.xMax < c_map.cellBounds.max.x) bounds.xMax = c_map.cellBounds.max.x;
                if (bounds.yMax < c_map.cellBounds.max.y) bounds.yMax = c_map.cellBounds.max.y;
                if (bounds.xMin > c_map.cellBounds.min.x) bounds.xMin = c_map.cellBounds.min.x;
                if (bounds.yMin > c_map.cellBounds.min.y) bounds.yMin = c_map.cellBounds.min.y;
            }
        }
    }

    /// <summary>
    /// Создание массивов тайлов и определение границ
    /// </summary>
    private void TilesArrayCreate()
    {
        TilesBoundsCreate();

        //Создаем границы наших массивов
        tilesGas = new TileGas[bounds.size.x, bounds.size.y];
        tilesBlock = new TileBlock[bounds.size.x, bounds.size.y];
        tilesDoor = new TileDoor[bounds.size.x, bounds.size.y];
        tilesPipe = new Tile[bounds.size.x, bounds.size.y];

        Debug.Log("Границы: " + bounds);

        //заполняем массив 
        for (int c_map = 0; c_map < map.Length; c_map++)
        {
            //сортируем объекты по массивам
            foreach (Transform c_object in map[c_map].transform)
            {
                //матрица координат
                int px = (int)(c_object.position.x + Mathf.Abs(bounds.xMin));
                int py = (int)(c_object.position.y + Mathf.Abs(bounds.yMin));
                //int pz = c_map;

                TileCheckMapAndAddToMatrix(px, py, c_map, c_object);
            }

            //Тайлмапа труб
            if (c_map == ((int)TileMapType.devices))
            {
                PipesSorting();
            }

        }
    }

    private void PipesSorting()
    {
        pipesNetwork = new List<TilePipeNetwork>();  //список готовых массивов систем труб

        int keyIntNetwork = 0;
        Vector2Int savedPipe = new Vector2Int(-1, -1);

        //находим тайлы труб и сохраняем их в общим массиве
        for (int px = bounds.xMin; px <= bounds.xMax; px++)
        {
            for (int py = bounds.yMin; py <= bounds.yMax; py++)
            {
                Vector3Int localPlace = new Vector3Int(px, py, 0);
                Vector3 place = map[(int)TileMapType.devices].CellToWorld(localPlace);
                Vector2Int tilePosition = new Vector2Int((int)(place.x + bounds.xMax + 1),
                                                         (int)(place.y + bounds.yMax));
                //Проверяем наличие тайла по позиции на тайлмапе
                if (map[(int)TileMapType.devices].HasTile(localPlace))
                {
                    bool isPipeAdded = false;

                    //сортируем тайлы по массивам
                    foreach (TilePipeNetwork network in pipesNetwork)
                    {   
                        //проверяем тайл на окружные тайлы
                        if (network.CheckPipesAround(tilePosition))
                        {
                            //вносим тайл в массив
                            network.AddPipe(tilePosition);
                            isPipeAdded = true;

                            //проверяем на пересечение с другими системами
                            foreach (TilePipeNetwork networkCheck in pipesNetwork)
                            {
                                if (network.key != networkCheck.key &&
                                    network.allPipes[tilePosition.x, tilePosition.y] == networkCheck.allPipes[tilePosition.x, tilePosition.y])
                                {
                                    //объединяем системы
                                    network.MergeNetwork(networkCheck);
                                }
                            }
                        }
                    }

                    //если тайл не был добавлен, то создаем новую систему труб
                    if (!isPipeAdded)
                    {
                        keyIntNetwork++;
                        TilePipeNetwork tilePipeNetwork = new TilePipeNetwork();
                        tilePipeNetwork.Initialize(keyIntNetwork, (Vector2Int)bounds.size);
                        tilePipeNetwork.AddPipe(tilePosition);

                        pipesNetwork.Add(tilePipeNetwork);
                    }
                }
            }
        }

        //запоминаем элементы для уборочного списка систем с которыми объединили 
        List<TilePipeNetwork> listForRemoveNetworks = new List<TilePipeNetwork>();
        //string debugStr = "В списке на исключение: ";
        foreach (TilePipeNetwork network in pipesNetwork)
        {
            //if (network.GetTrueList().Count == 0 && network.GetEndingPipesTrueList().Count == 0)
            if (network.GetTrueList().Count == 0)
            {
                //debugStr += $"[{network.key}], ";
                listForRemoveNetworks.Add(network);
            }
        }
        //Debug.Log(debugStr);

        //Исключаем элементы из списка систем
        foreach (TilePipeNetwork network in listForRemoveNetworks)
        {
            //Debug.Log($"Исключен [{network.key}]");
            pipesNetwork.Remove(network);
        }

        //Переопределяем ключи элементов в соответствии с новой нумерацией
        //И обновляем конечные точки
        keyIntNetwork = 0;
        foreach (TilePipeNetwork network in pipesNetwork)
        {
            //Debug.Log($"[{network.key}] изменил ключ на [{keyIntNetwork + 1}]");
            keyIntNetwork++;
            network.key = keyIntNetwork;

            network.UpdateEndingPipesTrueListAndVolume(bounds);
        }
    }

    /// <summary>
    /// Создание визуальной помощи в виде текста и дыма
    /// </summary>
    private void TilesArrayInitializeCreateVisualization()
    {
        //проверяем все объекты в слое пола
        foreach (Transform c_object in map[(int)TileMapType.floor].transform)
        {
            //матрица координат
            int px = (int)(c_object.position.x + Mathf.Abs(bounds.xMin));
            int py = (int)(c_object.position.y + Mathf.Abs(bounds.yMin));

            //создаем визуальные вспомогательные объекты
            tilesGas[px, py].CreateSmoke(); //создаем объект дыма на клетке
            tilesGas[px, py].CreateText(); //создаем объект текста на клетке
        }
    }

    /// <summary>
    /// Добавление нового тайла в матрицу
    /// </summary>
    public void TileAdd(Vector3Int position, int tileMapNumber)
    {
        foreach (Transform c_object in map[tileMapNumber].transform)
        {
            //позиция тайла из матрицы
            Vector2Int tilePosition = new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
                                                     (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));

            //проверяем схожа ли позиция найденного тайла с искомым тайлом, если нет, то продолжаем искать
            if (tilePosition != ((Vector2Int)position))
            {
                continue;
            }

            //Получаем объект и заносим его в нужный массив
            TileCheckMapAndAddToMatrix(tilePosition.x, tilePosition.y, tileMapNumber, c_object);
            Debug.Log($"В матрицу добавлен тайл {c_object.name}");
            break;
        }
    }
    /// <summary>
    /// ДУбирание тайла из матрицы
    /// </summary>
    public void TileRemove(Vector3Int positionTile, Vector3Int place, int tileMapNumber)
    {
        if (tileMapNumber == (int)TileMapType.doors)
        {
            //Debug.Log("Найден: " + tilesDoor[place.x, place.y].name);
            Destroy(tilesDoor[place.x, place.y]);
            //включаем дым под тайлом если он есть
            if (tilesGas[place.x, place.y].smokeObject.activeInHierarchy == false)
            {
                tilesGas[place.x, place.y].DeactivateBlockGas();
            }
        }
        else if (tileMapNumber == (int)TileMapType.blocks)
        {
            map[tileMapNumber].SetTile(positionTile, null); //убирание тайла стены и его GameObject
            map[tileMapNumber].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //Обновляем тайлмап коллайдер

            //включаем дым под тайлом если он есть
            if (tilesGas[place.x, place.y].smokeObject.activeInHierarchy == false)
            {
                tilesBlock[place.x, place.y].DeactivateBlockGas();
            }
        }
        else if (tileMapNumber == (int)TileMapType.floor)
        {
            for (int i = place.x - 1; i <= place.x + 1; i++)
            {
                for (int j = place.y - 1; j <= place.y + 1; j++)
                {
                    //tilesGas[place.x, place.y].
                    try
                    {
                        if (tilesGas[i, j] != null
                            && !tilesGas[place.x, place.y].CheckGasBlock(i, j)
                            && (Vector2Int)place != new Vector2Int(i, j))
                        {
                            tilesGas[i, j].pressure = 0;
                            tilesGas[i, j].TransmissionGas(tick_time, true);
                            //tilesGas[i, j].UpdateGas(-300, 0);
                            //UpdateTileMaps();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            map[tileMapNumber].SetTile(positionTile, null); //убираем тайл пола
        }
    }

    /// <summary>
    /// Добавление нового объекта в матрицу
    /// </summary>
    public void GameObjectAdd(Vector3Int position, int tileMapNumber)
    {
        foreach (Transform c_object in map[tileMapNumber].transform)
        {
            //позиция тайла из матрицы
            Vector2Int tilePosition = new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
                                                     (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));

            //проверяем схожа ли позиция найденного тайла с искомым тайлом, если нет, то продолжаем искать
            if (tilePosition != ((Vector2Int)position))
            {
                continue;
            }

            //Получаем объект и заносим его в нужный массив
            TileCheckMapAndAddToMatrix(tilePosition.x, tilePosition.y, tileMapNumber, c_object);
            Debug.Log($"В матрицу добавлен тайл {c_object.name}");
            break;
        }
    }

    /// <summary>
    /// Заменяет текущий тайл на новый тайл
    /// </summary>
    //public void TileReplace(Vector3Int position, int tileMapNumber)
    //{
    //    //позиция тайла из матрицы
    //    Vector2Int tilePosition = new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
    //                                             (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));

    //    //Получаем объект и заносим его в нужный массив

    //    TileCheckMapAndAddToMatrix(tilePosition.x, tilePosition.y, tileMapNumber, c_object);
    //    Debug.Log($"В матрице заменен тайл {c_object.name}");
    //}

    private void TileCheckMapAndAddToMatrix(int px, int py, int c_map, Transform c_object)
    {
        switch (c_map)
        {
            case (int)TileMapType.floor: //полы
                {
                    tilesGas[px, py] = c_object.GetComponent<TileGas>();
                    tilesGas[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileFloor");

                    //проверка на инициализацию, чтобы не создавались объекты под стенами и стеклами
                    if (isInitializeCompleted)
                    {
                        //создаем визуальные вспомогательные объекты
                        tilesGas[px, py].CreateSmoke(); //создаем объект дыма на клетке
                        tilesGas[px, py].CreateText(); //создаем объект текста на клетке
                    }
                    break;
                }
            case (int)TileMapType.blocks: //стены, окна
                {
                    tilesBlock[px, py] = c_object.GetComponent<TileBlock>(); //запоминаем компонент в массиве
                    tilesBlock[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileBlock");
                    break;
                }
            case (int)TileMapType.doors: //двери
                {
                    //создаем визуальные вспомогательные объекты под дверьми
                    tilesGas[px, py].CreateSmoke(); //создаем объект дыма на клетке
                    tilesGas[px, py].CreateText(); //создаем объект текста на клетке

                    tilesDoor[px, py] = c_object.gameObject.GetComponent<TileDoor>(); //запоминаем объект в массиве
                    tilesDoor[px, py].InitializeDoor(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileDoor"); //запускаем обработчик дверей
                    break;
                }
            //case (int)TileMapType.devices: //девайсы
            //    {
            //        Debug.Log($"Найдены девайсы: {c_object}, {c_object.name} [{px};{py}]");
            //        break;
            //    }
        }
    }

    public void TestMethod()
    {
        //FindObjectOfType<ProjectSaveLoad>().SaveAllData();
        //FindObjectOfType<ProjectSaveLoad>().LoadAllData();


        //PlayerController player = FindObjectOfType<PlayerController>();
        //player.Damage(5);


        ////тестовый визуализатор систем труб
        //Color32 testColor = new Color32();
        //int testCount = 0;
        //int colValue = 100;
        //foreach (TilePipeNetwork pipes in pipesNetwork)
        //{
        //    //смена цветов
        //    int colorRGB = colValue * testCount;
        //    testCount++;
        //    testColor = new Color32(
        //        (byte)Mathf.Clamp(colorRGB > 255 ? colorRGB/3 : colorRGB, 0, 255),
        //        (byte)Mathf.Clamp(colorRGB > 255 ? colorRGB/3 : colorRGB, 0, 255),
        //        (byte)Mathf.Clamp(colorRGB > 255 ? colorRGB/3 : colorRGB, 0, 255),
        //        (byte)255);
        //    foreach (Vector2Int tile in pipes.GetTrueList())
        //    {
        //        tilesGas[tile.x, tile.y].smokeObject.GetComponent<SpriteRenderer>().color = testColor;
        //    }

        //    //Debug.Log(pipes.GetInfo());

        //    string result = $"Окончания для {pipes.key}: ";
        //    foreach (Vector2Int pipe in pipes.GetEndingPipesTrueList())
        //    {
        //        result += $"[{pipe}], ";
        //    }
        //    result += $"\n Список всех элементов: ";
        //    foreach (Vector2Int pipe in pipes.GetTrueList())
        //    {
        //        result += $"[{pipe}], ";
        //    }
        //    Debug.Log(result);
        //}
    }
}

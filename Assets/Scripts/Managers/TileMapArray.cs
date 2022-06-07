using System;
using System.Collections;
using System.Collections.Generic;
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
    public List<Tilemap> devices; //массив всех объектов-девайсов
    public List<TilePipeNetwork> pipesNetwork; //список систем массива труб, внутри которых находятся входы труб

    HashSet<GameObject> hashObjects;

    public BoundsInt bounds; //границы массива и тайлов

    Camera mainCamera;

    [Header("Рассчетные переменные")]
    public float tick_time; //Время в секундах для повтора
    [SerializeField] float tick_curr_time; //Текущее время
    public bool isNeedUpdateArray = false; //проверка, необходимо ли обновление или можно не выполнять метод UpdateArray()
    [SerializeField] int countActivate; //Счет невозможных тайлов для обновление

    //Типы ТайлМапов по нумерациям !!!лучше переделать с определением типа из получаемого списка!!!
    enum TileMapType
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

    void FixedUpdate()
    {
        //Запуск обновления тайлов газа
        //if (isNeedUpdateArray) UpdateArray(); -- !!!временно убрано из-за бага деактивации!!!
        UpdateArray();
    }

    /// <summary>
    /// Обновление массива тайлов по границам
    /// </summary>
    private void UpdateArray()
    {
        tick_curr_time -= Time.deltaTime; // Вычитаем время кадра
        if (tick_curr_time <= 0) //Время вышло
        {
            countActivate = 0;
            for (int i = 0; i < tilesGas.Length; i++)
            {
                for (int j = 0; j < tilesGas.Length; j++)
                {
                    try
                    {
                        if (tilesGas[i, j] != null)
                        {
                            tilesGas[i, j].PressureTransmission(tick_time);

                            if (tilesGas[i, j].isActive)
                            {
                                countActivate++;
                                isNeedUpdateArray = true;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            tick_curr_time = tick_time; // повторный запуск таймера
            isNeedUpdateArray = countActivate == 0 ? false : true;
        }
    }

    /// <summary>
    /// Устанавливаем границы по всем объектам тайлмапа
    /// </summary>
    private void TilesBoundsCreate()
    {
        //установление границ
        map[0].CompressBounds();
        bounds = map[0].cellBounds;
        foreach (Tilemap c_map in map)
        {
            if (c_map != map[0])
            {
                c_map.CompressBounds();
                if (bounds.xMax < c_map.cellBounds.max.x) bounds.xMax = c_map.cellBounds.max.x;
                if (bounds.yMax < c_map.cellBounds.max.y) bounds.yMax = c_map.cellBounds.max.y;
                if (bounds.xMin > c_map.cellBounds.min.x) bounds.xMin = c_map.cellBounds.min.x;
                if (bounds.yMin > c_map.cellBounds.min.y) bounds.yMin = c_map.cellBounds.min.y;
            }
        }

        //!!!!!Примерно такое же должно быть обновление границ, но с переносом всех текущих массивов данных
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

            network.UpdateEndingPipesTrueList();
        }
    }

    /// <summary>
    /// Создание визуальной помощи в виде текста и дыма
    /// </summary>
    private void TilesArrayInitializeCreateVisualization()
    {
        //проверяем все объекты в слое пола
        foreach (Transform c_object in map[0].transform)
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
            case (int)TileMapType.devices: //девайсы
                {
                    Debug.Log($"Найдены девайсы: {c_object}, {c_object.name} [{px};{py}]");
                    break;
                }
        }
    }

    public void TestMethod()
    {
        //foreach (TileDoor door in FindObjectsOfType<TileDoor>())
        //{
        //    hashObjects.Add(door.gameObject);
        //    Debug.Log($"В хеш добавлен объект{ door.name }");
        //    tilesDoor[1, 1] = door.GetComponent<TileDoor>(); //запоминаем объект в массиве
        //}


        //тестовый визуализатор систем труб
        Color32 testColor = new Color32();
        int testCount = 0;
        int colValue = 100;
        foreach (TilePipeNetwork pipes in pipesNetwork)
        {
            //смена цветов
            int colorRGB = colValue * testCount;
            testCount++;
            testColor = new Color32(
                (byte)Mathf.Clamp(colorRGB > 255 ? colorRGB/3 : colorRGB, 0, 255),
                (byte)Mathf.Clamp(colorRGB > 255 ? colorRGB/3 : colorRGB, 0, 255),
                (byte)Mathf.Clamp(colorRGB > 255 ? colorRGB/3 : colorRGB, 0, 255),
                (byte)255);
            foreach (Vector2Int tile in pipes.GetTrueList())
            {
                tilesGas[tile.x, tile.y].smokeObject.GetComponent<SpriteRenderer>().color = testColor;
            }

            //Debug.Log(pipes.GetInfo());

            string result = $"Окончания для {pipes.key}: ";
            foreach (Vector2Int pipe in pipes.GetEndingPipesTrueList())
            {
                result += $"[{pipe}], ";
            }
            result += $"\n Список всех элементов: ";
            foreach (Vector2Int pipe in pipes.GetTrueList())
            {
                result += $"[{pipe}], ";
            }
            Debug.Log(result);
        }
    }
}

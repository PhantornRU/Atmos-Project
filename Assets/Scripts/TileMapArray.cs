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
    //public GameObject[,] tilesArray; //массив всех тайлов //заменено TileGas[,] на GameObject. Ибо ну черт его знает зачем запоминать только один скрипт, если можно весь объект. По оптимизации - хз
    public TileGas[,] tilesGas; //массив газовых тайлов
    public TileBlock[,] tilesBlock; //массив блоковых тайлов - стены, стекло
    public TileDoor[,] tilesDoor; //массив объектов тайлов - двери
    public Tile[,] tilesPipe; //массив объектов тайлов - трубы, их девайсы
    public List<Vector3[,]> pipesNetwork; //список систем массива труб, внутри которых находятся входы труб

    HashSet<GameObject> hashObjects;

    public BoundsInt bounds; //границы массива и тайлов

    Camera mainCamera;

    [Header("Рассчетные переменные")]
    public float tick_time; //Время в секундах для повтора
    [SerializeField] float tick_curr_time; //Текущее время
    public bool isNeedUpdateArray = false; //проверка, необходимо ли обновление или можно не выполнять метод UpdateArray()
    [SerializeField] int countActivate; //Счет невозможных тайлов для обновление

    //Номера тайлмап
    const int numberFloors = 0;
    const int numberWallsAndWindows = 1;
    const int numberDoors = 2;
    const int numberPipes = 3;

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
        if (isNeedUpdateArray) UpdateArray();
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

    [SerializeField] List<Vector3> pipesNetwork2; //тест

    /// <summary>
    /// Создание массивов тайлов и определение границ
    /// </summary>
    private void TilesArrayCreate()
    {
        pipesNetwork2 = new List<Vector3>(); //тест  !!!!ПОТОМ УБРАТЬ!!!

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
            foreach (Transform c_object in map[c_map].transform)
            {
                //матрица координат
                int px = (int)(c_object.position.x + Mathf.Abs(bounds.xMin));
                int py = (int)(c_object.position.y + Mathf.Abs(bounds.yMin));
                //int pz = c_map;

                TileCheckMapAndAddToMatrix(px, py, c_map, c_object);
            }

            //Тайлмапа труб
            if (c_map == numberPipes)
            {
                //List<Vector3> sortingPipesNetwork = new List<Vector3>();
                pipesNetwork = new List<Vector3[,]>();
                int zMax = 10; //максимум количества систем, временное значение
                Vector3[,] sortingPipesNetwork = new Vector3[bounds.xMax, bounds.yMax]; // общим массив который надо отсортировать
                Vector3[,,] sortingSavedPipesNetwork = new Vector3[bounds.xMax, bounds.yMax, zMax]; // массивы, где каждый Z-уровень - система труб
                //pipesNetwork.Add(sortingPipesNetwork);

                Vector3Int placeSort = new Vector3Int(0, 0, 0);// = sortingPipesNetwork.; //для запоминания выбора сортировки

                Debug.Log("1");//тестовый   !!!!ПОТОМ УБРАТЬ!!!

                for (int px = bounds.xMin; px < bounds.xMax; px++)
                {
                    for (int py = bounds.yMin; py < bounds.yMax; py++)
                    {
                        Vector3Int localPlace = new Vector3Int(px, py, 0);
                        Vector3 place = map[c_map].CellToWorld(localPlace);
                        Vector3Int tilePosition = new Vector3Int((int)(place.x + bounds.xMax + 1), 
                                                                 (int)(place.y + bounds.yMax), 0);
                        Debug.Log("2");//тестовый   !!!!ПОТОМ УБРАТЬ!!!
                        //Проверяем наличие тайла по позиции на тайлмапе
                        if (map[c_map].HasTile(localPlace))
                        {
                            Debug.Log("3");//тестовый   !!!!ПОТОМ УБРАТЬ!!!
                            sortingPipesNetwork[tilePosition.x, tilePosition.y] = tilePosition; //!!!!!!!!!!!ТУТ ПРОБЛЕМА!!!!!!!!!!!!!!!!!!! скорее всего неправильно следующую позицию берет
                            placeSort = tilePosition;

                            pipesNetwork2.Add(tilePosition); //добавляем наш тайл в список  !!!!ПОТОМ УБРАТЬ!!!
                            Debug.Log("4");//тестовый   !!!!ПОТОМ УБРАТЬ!!!
                        }
                    }
                }


                //Сортируем найденные объекты по сетям труб
                bool checkNeedExit = false;
                Vector3Int placeLast = placeSort; //последний сохраненный
                int zSaved = 0; //количество созданных слоев массивов сети труб
                do
                {
                    bool checkBreak = false; //проверка на выход из поиска тайлов
                    for (int px = -1; px <= 1; px++)
                    {
                        for (int py = -1; py <= 1; py++)
                        {
                            //проверяем чтобы значения не являлись самим собой и не было нулями (!без проверки выходят ли они за массив и меньше ли нуля, надо бы добавить!)
                            if (!(px == 0 && py == 0)
                                && !(placeLast.x + px == 0 && placeLast.y + py == 0)
                                && !(placeLast.x == 0 && placeLast.y == 0))
                            {
                                //проверяем нахождение тайла по заданной позиции на всех слоях
                                for (int pz = 0; pz <= zSaved; pz++)
                                {
                                    if (sortingSavedPipesNetwork[placeLast.x + px, placeLast.y + py, pz] != null)
                                    {
                                        Debug.Log("5");//тестовый   !!!!ПОТОМ УБРАТЬ!!!
                                        //запоминаем на текущем слое
                                        sortingSavedPipesNetwork[placeLast.x, placeLast.y, pz] = new Vector3(placeLast.x, placeLast.y, pz);

                                        //очищаем общий массив для сортировки
                                        sortingPipesNetwork[placeLast.x, placeLast.y] = new Vector3(0, 0);
                                        pipesNetwork2.Remove(placeLast); //убираем элемент из тестового листа  !!!!ПОТОМ УБРАТЬ!!!

                                        //ищем следующий элемент с которого будем начинать следующий поиск
                                        bool _checkBreak = false;
                                        for (int _px = -1; _px <= 1; _px++)
                                        {
                                            for (int _py = -1; _py <= 1; _py++)
                                            {
                                                if (sortingSavedPipesNetwork[placeLast.x + _px, placeLast.y + _py, 0] != null)
                                                {
                                                    placeLast = new Vector3Int((int)(placeLast.x + _px), (int)(placeLast.y + _py), pz);
                                                    _checkBreak = true;
                                                    break;
                                                }    
                                            }
                                            if (_checkBreak) break; //завершаем поиск
                                        }
                                        
                                        //выходим из проверки остальных позиций
                                        checkBreak = true;
                                        break;
                                    }
                                }

                                if (checkBreak) break; //завершаем поиск, иначе...

                                //Тайл не найден на слоях, поэтому сохраняем его на новом слое. Если труба одна, то для него будет отдельный слой
                                //...............................................................................
                                //.............................ТУТ ТИПА КОД......................................
                                //...............................................................................
                                //!!!!!ЗДЕСЬ МЫ ДОЛЖНЫ ДЕЛАТЬ ТО ЖЕ ЧТО И НАВЕРХУ, НО С НУЛЕВЫМ СЛОЕМ ИЛИ СОЗДАНИЕМ ОТДЕЛЬНОГО СЛОЯ!!!!!!!

                                //т.е. даже искать не надо, мы сразу ищем по нулевому если он не пустой
                                if (sortingPipesNetwork[placeLast.x + px, placeLast.y + py] != null)
                                {
                                    Debug.Log("6");//тестовый   !!!!ПОТОМ УБРАТЬ!!!
                                    //запоминаем на текущем слое
                                    sortingSavedPipesNetwork[placeLast.x, placeLast.y, 0] = new Vector3(placeLast.x, placeLast.y, 0);

                                    //очищаем общий массив для сортировки
                                    sortingPipesNetwork[placeLast.x, placeLast.y] = new Vector3(0, 0);
                                    pipesNetwork2.Remove(placeLast); //убираем элемент из тестового листа  !!!!ПОТОМ УБРАТЬ!!!

                                    //Запоминаем следующий
                                    placeLast = new Vector3Int((int)(placeLast.x + px), (int)(placeLast.y + py), 0);
                                }
                            }

                            if (checkBreak) break; //завершаем поиск

                        }
                        if (checkBreak) break;
                    }

                    Debug.Log("7");//тестовый   !!!!ПОТОМ УБРАТЬ!!!

                    //задаем что пора выходить
                    if (pipesNetwork2.Count == 0)
                    {
                        checkNeedExit = true;
                    }
                }
                while (!checkNeedExit);


                Debug.Log("8");//тестовый   !!!!ПОТОМ УБРАТЬ!!!





                //!!!!!По завершению все массивы переделываются в листы и добавляются в список

                //List<Vector3> listLast = ;

                //string test = "Тест тайлмапа: ";
                //test += $"{_tile} name: {_tile.name}; ";
                //Debug.Log(test);
            }
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
            case numberFloors: //полы
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
            case numberWallsAndWindows: //стены, окна
                {
                    tilesBlock[px, py] = c_object.GetComponent<TileBlock>(); //запоминаем компонент в массиве
                    tilesBlock[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileBlock");
                    break;
                }
            case numberDoors: //двери
                {
                    tilesDoor[px, py] = c_object.gameObject.GetComponent<TileDoor>(); //запоминаем объект в массиве
                    tilesDoor[px, py].InitializeDoor(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileDoor"); //запускаем обработчик дверей
                    break;
                }
            case numberPipes: //девайсы к трубам
                {
                    Debug.Log($"Найден девайс тайла трубы: {c_object}, {c_object.name} [{px};{py}]");
                    break;
                }
        }
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

    public void TestMethod()
    {
        foreach (TileDoor door in FindObjectsOfType<TileDoor>())
        {
            hashObjects.Add(door.gameObject);
            Debug.Log($"В хеш добавлен объект{ door.name }");
            tilesDoor[1, 1] = door.GetComponent<TileDoor>(); //запоминаем объект в массиве
        }
    }
}

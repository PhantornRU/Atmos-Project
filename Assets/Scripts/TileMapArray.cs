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

    HashSet<GameObject> hashObjects;

    public BoundsInt bounds; //границы массива и тайлов

    Camera mainCamera;

    [Header("Рассчетные переменные")]
    public float tick_time; //Время в секундах для повтора
    [SerializeField] float tick_curr_time; //Текущее время
    public bool isNeedUpdateArray = false; //проверка, необходимо ли обновление или можно не выполнять метод UpdateArray()
    [SerializeField] int countActivate; //Счет невозможных тайлов для обновление

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
            case 0: //полы
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
            case 1: //стены, окна
                {
                    tilesBlock[px, py] = c_object.GetComponent<TileBlock>(); //запоминаем компонент в массиве
                    tilesBlock[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileBlock");
                    break;
                }
            case 2: //двери
                {
                    tilesDoor[px, py] = c_object.gameObject.GetComponent<TileDoor>(); //запоминаем объект в массиве
                    tilesDoor[px, py].InitializeDoor(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileDoor"); //запускаем обработчик дверей
                    break;
                }
            case 3: //трубы
                {
                    Debug.Log($"Найден тайл трубы: {c_object}, {c_object.name} [{px};{py}]");
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

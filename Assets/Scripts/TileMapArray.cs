using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//� �������� ����� ������� ������� ��� � ���� ��������� ��� ������ � �������� ����� ����� ���� ���,
//������ �������� ���� �������� ����� �������

//Grid Map
[DisallowMultipleComponent]
public class TileMapArray : MonoBehaviour
{
    [Header("�������")]
    public Tilemap[] map; //������ ���� ����� ����
    //public GameObject[,] tilesArray; //������ ���� ������ //�������� TileGas[,] �� GameObject. ��� �� ���� ��� ����� ����� ���������� ������ ���� ������, ���� ����� ���� ������. �� ����������� - ��
    public TileGas[,] tilesGas; //������ ������� ������
    public TileBlock[,] tilesBlock; //������ �������� ������ - �����, ������
    public TileDoor[,] tilesDoor; //������ �������� ������ - �����
    public Tile[,] tilesPipe; //������ �������� ������ - �����, �� �������
    public List<Vector3[,]> pipesNetwork; //������ ������ ������� ����, ������ ������� ��������� ����� ����

    HashSet<GameObject> hashObjects;

    public BoundsInt bounds; //������� ������� � ������

    Camera mainCamera;

    [Header("���������� ����������")]
    public float tick_time; //����� � �������� ��� �������
    [SerializeField] float tick_curr_time; //������� �����
    public bool isNeedUpdateArray = false; //��������, ���������� �� ���������� ��� ����� �� ��������� ����� UpdateArray()
    [SerializeField] int countActivate; //���� ����������� ������ ��� ����������

    //������ �������
    const int numberFloors = 0;
    const int numberWallsAndWindows = 1;
    const int numberDoors = 2;
    const int numberPipes = 3;

    private bool isInitializeCompleted = false; //�������� �� ���������� �������������

    public void Initialize(float _tick_time)
    {
        tick_time = _tick_time;
        mainCamera = Camera.main;

        //��������� ��� ������ ����������� �������
        TilesArrayCreate();

        //������������� ������ �� �����
        TilesArrayInitializeCreateVisualization();

        hashObjects = new HashSet<GameObject>();

        isInitializeCompleted = true;
    }

    void FixedUpdate()
    {
        //������ ���������� ������ ����
        if (isNeedUpdateArray) UpdateArray();
    }

    /// <summary>
    /// ������������� ������� �� ���� �������� ��������
    /// </summary>
    private void TilesBoundsCreate()
    {
        //������������ ������
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

        //!!!!!�������� ����� �� ������ ���� ���������� ������, �� � ��������� ���� ������� �������� ������
    }

    [SerializeField] List<Vector3> pipesNetwork2; //����

    /// <summary>
    /// �������� �������� ������ � ����������� ������
    /// </summary>
    private void TilesArrayCreate()
    {
        pipesNetwork2 = new List<Vector3>(); //����  !!!!����� ������!!!

        TilesBoundsCreate();

        //������� ������� ����� ��������
        tilesGas = new TileGas[bounds.size.x, bounds.size.y];
        tilesBlock = new TileBlock[bounds.size.x, bounds.size.y];
        tilesDoor = new TileDoor[bounds.size.x, bounds.size.y];
        tilesPipe = new Tile[bounds.size.x, bounds.size.y];

        Debug.Log("�������: " + bounds);

        //��������� ������ 
        for (int c_map = 0; c_map < map.Length; c_map++)
        {
            foreach (Transform c_object in map[c_map].transform)
            {
                //������� ���������
                int px = (int)(c_object.position.x + Mathf.Abs(bounds.xMin));
                int py = (int)(c_object.position.y + Mathf.Abs(bounds.yMin));
                //int pz = c_map;

                TileCheckMapAndAddToMatrix(px, py, c_map, c_object);
            }

            //�������� ����
            if (c_map == numberPipes)
            {
                //List<Vector3> sortingPipesNetwork = new List<Vector3>();
                pipesNetwork = new List<Vector3[,]>();
                int zMax = 10; //�������� ���������� ������, ��������� ��������
                Vector3[,] sortingPipesNetwork = new Vector3[bounds.xMax, bounds.yMax];
                Vector3[,,] sortingSavedPipesNetwork = new Vector3[bounds.xMax, bounds.yMax, zMax];
                //pipesNetwork.Add(sortingPipesNetwork);

                Vector3Int placeSort = new Vector3Int(0, 0, 0);// = sortingPipesNetwork.; //��� ����������� ������ ����������

                for (int n = bounds.xMin; n < bounds.xMax; n++)
                {
                    for (int p = bounds.yMin; p < bounds.yMax; p++)
                    {
                        Vector3Int localPlace = new Vector3Int(n, p, 0);
                        Vector3 place = map[c_map].CellToWorld(localPlace);
                        Vector3Int tilePosition = new Vector3Int((int)(place.x + bounds.xMax + 1),
                                                                      (int)(place.y + bounds.yMax), 0);

                        if (map[c_map].HasTile(localPlace))
                        {
                            sortingPipesNetwork[tilePosition.x, tilePosition.y] = tilePosition;
                            placeSort = tilePosition;

                            pipesNetwork2.Add(tilePosition); //��������� ��� ���� � ������  !!!!����� ������!!!
                        }
                    }
                }


                //��������� ��������� ������� �� ����� ����
                bool checkNeedExit = false;
                do
                {
                    Vector3 placeLast = sortingPipesNetwork[placeSort.x, placeSort.y];
                    int zSaved = 0;
                    for (int px = -1; px <= 1; px++)
                    {
                        bool checkBreak = false;
                        for (int py = -1; py <= 1; py++)
                        {
                            if (placeSort.x + px != 0 && placeSort.y + py != 0 //�������� ���� ���� ��������� ||
                                && placeSort.x != 0 && placeSort.y != 0)
                            {
                                if (sortingPipesNetwork[placeSort.x + px, placeSort.y + py] != null)
                                {
                                    sortingSavedPipesNetwork[placeSort.x, placeSort.y, zSaved] = placeLast;
                                    placeLast = sortingPipesNetwork[placeSort.x + px, placeSort.y + py];
                                    pipesNetwork2.Remove(placeSort);

                                    checkBreak = true; //������� �� �������� ��������� �������
                                    break;
                                }
                                else
                                {
                                    //��������� �� ������������ �� �� � ������� ������ � �� �������� �� �� ������������
                                    //���������� �� �� ��������, �� ��� ��� ������� ����
                                    for (int pz = 0; pz <= zMax; pz++)
                                    {
                                        if (sortingSavedPipesNetwork[placeSort.x + px, placeSort.y + py, pz] != null)
                                        {
                                            sortingSavedPipesNetwork[placeSort.x, placeSort.y, pz] = placeLast;
                                            placeLast = sortingSavedPipesNetwork[placeSort.x + px, placeSort.y + py, pz];
                                            pipesNetwork2.Remove(placeSort);

                                            checkBreak = true; //������� �� �������� ��������� �������
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (checkBreak) break;
                    }

                    //������ ��� ���� ��������
                    if (pipesNetwork2.Count == 0)
                    {
                        checkNeedExit = true;
                    }
                }
                while (!checkNeedExit);







                //!!!!!�� ���������� ��� ������� �������������� � ����� � ����������� � ������

                //List<Vector3> listLast = ;

                //string test = "���� ��������: ";
                //test += $"{_tile} name: {_tile.name}; ";
                //Debug.Log(test);
            }
        }
    }

    /// <summary>
    /// �������� ���������� ������ � ���� ������ � ����
    /// </summary>
    private void TilesArrayInitializeCreateVisualization()
    {
        //��������� ��� ������� � ���� ����
        foreach (Transform c_object in map[0].transform)
        {
            //������� ���������
            int px = (int)(c_object.position.x + Mathf.Abs(bounds.xMin));
            int py = (int)(c_object.position.y + Mathf.Abs(bounds.yMin));

            //������� ���������� ��������������� �������
            tilesGas[px, py].CreateSmoke(); //������� ������ ���� �� ������
            tilesGas[px, py].CreateText(); //������� ������ ������ �� ������
        }
    }

    /// <summary>
    /// ���������� ������ ����� � �������
    /// </summary>
    public void TileAdd(Vector3Int position, int tileMapNumber)
    {
        foreach (Transform c_object in map[tileMapNumber].transform)
        {
            //������� ����� �� �������
            Vector2Int tilePosition = new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
                                                     (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));

            //��������� ����� �� ������� ���������� ����� � ������� ������, ���� ���, �� ���������� ������
            if (tilePosition != ((Vector2Int)position))
            {
                continue;
            }

            //�������� ������ � ������� ��� � ������ ������

            TileCheckMapAndAddToMatrix(tilePosition.x, tilePosition.y, tileMapNumber, c_object);
            Debug.Log($"� ������� �������� ���� {c_object.name}");
            break;
        }
    }

    private void TileCheckMapAndAddToMatrix(int px, int py, int c_map, Transform c_object)
    {
        switch (c_map)
        {
            case numberFloors: //����
                {
                    tilesGas[px, py] = c_object.GetComponent<TileGas>();
                    tilesGas[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileFloor");

                    //�������� �� �������������, ����� �� ����������� ������� ��� ������� � ��������
                    if (isInitializeCompleted)
                    {
                        //������� ���������� ��������������� �������
                        tilesGas[px, py].CreateSmoke(); //������� ������ ���� �� ������
                        tilesGas[px, py].CreateText(); //������� ������ ������ �� ������
                    }
                    break;
                }
            case numberWallsAndWindows: //�����, ����
                {
                    tilesBlock[px, py] = c_object.GetComponent<TileBlock>(); //���������� ��������� � �������
                    tilesBlock[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileBlock");
                    break;
                }
            case numberDoors: //�����
                {
                    tilesDoor[px, py] = c_object.gameObject.GetComponent<TileDoor>(); //���������� ������ � �������
                    tilesDoor[px, py].InitializeDoor(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileDoor"); //��������� ���������� ������
                    break;
                }
            case numberPipes: //������� � ������
                {
                    Debug.Log($"������ ������ ����� �����: {c_object}, {c_object.name} [{px};{py}]");
                    break;
                }
        }
    }

    /// <summary>
    /// ���������� ������� ������ �� ��������
    /// </summary>
    private void UpdateArray()
    {
        tick_curr_time -= Time.deltaTime; // �������� ����� �����
        if (tick_curr_time <= 0) //����� �����
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
            tick_curr_time = tick_time; // ��������� ������ �������
            isNeedUpdateArray = countActivate == 0 ? false : true;
        }
    }

    public void TestMethod()
    {
        foreach (TileDoor door in FindObjectsOfType<TileDoor>())
        {
            hashObjects.Add(door.gameObject);
            Debug.Log($"� ��� �������� ������{ door.name }");
            tilesDoor[1, 1] = door.GetComponent<TileDoor>(); //���������� ������ � �������
        }
    }
}

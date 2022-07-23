using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //������� � ���������
    public TileGas[,] tilesGas; //������ ������� ������
    public TileBlock[,] tilesBlock; //������ �������� ������ - �����, ������
    public TileDoor[,] tilesDoor; //������ �������� ������ - �����
    public Tile[,] tilesPipe; //������ �������� ������ - �����, �� �������
    public List<TilePipeNetwork> pipesNetwork; //������ ������ ������� ����, ������ ������� ��������� ����� ����

    HashSet<GameObject> hashObjects;

    public BoundsInt bounds; //������� ������� � ������

    Camera mainCamera;

    [Header("���������� ����������")]
    private float tick_time; //����� � �������� ��� �������
    public int countActivate; //���� ����������� ������ ��� ����������

    //���� ��������� �� ���������� !!!����� ���������� � ������������ ���� �� ����������� ������!!!
    public enum TileMapType
    {
        floor,  //0
        blocks, //1 walls & windows
        doors,  //2
        devices   //3

    }

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

    [HideInInspector] public bool isUpdateVisual = true;

    public void ToggleVisual()
    {
        isUpdateVisual = !isUpdateVisual;

        //����������� ��� ���� � ������ � �����
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
    /// ���������� ������� ������ �� ��������
    /// </summary>
    public void UpdateTileMaps()
    {
        //������ ���������� ������ ����
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

        //��������� ������� ���� � ��������� �����
        foreach (TilePipeNetwork network in pipesNetwork)
        {
            network.UpdatePipeNetwork();
        }
    }

    /// <summary>
    /// ������������� ������� �� ���� �������� ��������
    /// </summary>
    private void TilesBoundsCreate()
    {
        //������������ ������
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
    /// �������� �������� ������ � ����������� ������
    /// </summary>
    private void TilesArrayCreate()
    {
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
            //��������� ������� �� ��������
            foreach (Transform c_object in map[c_map].transform)
            {
                //������� ���������
                int px = (int)(c_object.position.x + Mathf.Abs(bounds.xMin));
                int py = (int)(c_object.position.y + Mathf.Abs(bounds.yMin));
                //int pz = c_map;

                TileCheckMapAndAddToMatrix(px, py, c_map, c_object);
            }

            //�������� ����
            if (c_map == ((int)TileMapType.devices))
            {
                PipesSorting();
            }

        }
    }

    private void PipesSorting()
    {
        pipesNetwork = new List<TilePipeNetwork>();  //������ ������� �������� ������ ����

        int keyIntNetwork = 0;
        Vector2Int savedPipe = new Vector2Int(-1, -1);

        //������� ����� ���� � ��������� �� � ����� �������
        for (int px = bounds.xMin; px <= bounds.xMax; px++)
        {
            for (int py = bounds.yMin; py <= bounds.yMax; py++)
            {
                Vector3Int localPlace = new Vector3Int(px, py, 0);
                Vector3 place = map[(int)TileMapType.devices].CellToWorld(localPlace);
                Vector2Int tilePosition = new Vector2Int((int)(place.x + bounds.xMax + 1),
                                                         (int)(place.y + bounds.yMax));
                //��������� ������� ����� �� ������� �� ��������
                if (map[(int)TileMapType.devices].HasTile(localPlace))
                {
                    bool isPipeAdded = false;

                    //��������� ����� �� ��������
                    foreach (TilePipeNetwork network in pipesNetwork)
                    {   
                        //��������� ���� �� �������� �����
                        if (network.CheckPipesAround(tilePosition))
                        {
                            //������ ���� � ������
                            network.AddPipe(tilePosition);
                            isPipeAdded = true;

                            //��������� �� ����������� � ������� ���������
                            foreach (TilePipeNetwork networkCheck in pipesNetwork)
                            {
                                if (network.key != networkCheck.key &&
                                    network.allPipes[tilePosition.x, tilePosition.y] == networkCheck.allPipes[tilePosition.x, tilePosition.y])
                                {
                                    //���������� �������
                                    network.MergeNetwork(networkCheck);
                                }
                            }
                        }
                    }

                    //���� ���� �� ��� ��������, �� ������� ����� ������� ����
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

        //���������� �������� ��� ���������� ������ ������ � �������� ���������� 
        List<TilePipeNetwork> listForRemoveNetworks = new List<TilePipeNetwork>();
        //string debugStr = "� ������ �� ����������: ";
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

        //��������� �������� �� ������ ������
        foreach (TilePipeNetwork network in listForRemoveNetworks)
        {
            //Debug.Log($"�������� [{network.key}]");
            pipesNetwork.Remove(network);
        }

        //�������������� ����� ��������� � ������������ � ����� ����������
        //� ��������� �������� �����
        keyIntNetwork = 0;
        foreach (TilePipeNetwork network in pipesNetwork)
        {
            //Debug.Log($"[{network.key}] ������� ���� �� [{keyIntNetwork + 1}]");
            keyIntNetwork++;
            network.key = keyIntNetwork;

            network.UpdateEndingPipesTrueListAndVolume(bounds);
        }
    }

    /// <summary>
    /// �������� ���������� ������ � ���� ������ � ����
    /// </summary>
    private void TilesArrayInitializeCreateVisualization()
    {
        //��������� ��� ������� � ���� ����
        foreach (Transform c_object in map[(int)TileMapType.floor].transform)
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
    /// <summary>
    /// ��������� ����� �� �������
    /// </summary>
    public void TileRemove(Vector3Int positionTile, Vector3Int place, int tileMapNumber)
    {
        if (tileMapNumber == (int)TileMapType.doors)
        {
            //Debug.Log("������: " + tilesDoor[place.x, place.y].name);
            Destroy(tilesDoor[place.x, place.y]);
            //�������� ��� ��� ������ ���� �� ����
            if (tilesGas[place.x, place.y].smokeObject.activeInHierarchy == false)
            {
                tilesGas[place.x, place.y].DeactivateBlockGas();
            }
        }
        else if (tileMapNumber == (int)TileMapType.blocks)
        {
            map[tileMapNumber].SetTile(positionTile, null); //�������� ����� ����� � ��� GameObject
            map[tileMapNumber].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //��������� ������� ���������

            //�������� ��� ��� ������ ���� �� ����
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
            map[tileMapNumber].SetTile(positionTile, null); //������� ���� ����
        }
    }

    /// <summary>
    /// ���������� ������ ������� � �������
    /// </summary>
    public void GameObjectAdd(Vector3Int position, int tileMapNumber)
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

    /// <summary>
    /// �������� ������� ���� �� ����� ����
    /// </summary>
    //public void TileReplace(Vector3Int position, int tileMapNumber)
    //{
    //    //������� ����� �� �������
    //    Vector2Int tilePosition = new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
    //                                             (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));

    //    //�������� ������ � ������� ��� � ������ ������

    //    TileCheckMapAndAddToMatrix(tilePosition.x, tilePosition.y, tileMapNumber, c_object);
    //    Debug.Log($"� ������� ������� ���� {c_object.name}");
    //}

    private void TileCheckMapAndAddToMatrix(int px, int py, int c_map, Transform c_object)
    {
        switch (c_map)
        {
            case (int)TileMapType.floor: //����
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
            case (int)TileMapType.blocks: //�����, ����
                {
                    tilesBlock[px, py] = c_object.GetComponent<TileBlock>(); //���������� ��������� � �������
                    tilesBlock[px, py].Initialize(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileBlock");
                    break;
                }
            case (int)TileMapType.doors: //�����
                {
                    //������� ���������� ��������������� ������� ��� �������
                    tilesGas[px, py].CreateSmoke(); //������� ������ ���� �� ������
                    tilesGas[px, py].CreateText(); //������� ������ ������ �� ������

                    tilesDoor[px, py] = c_object.gameObject.GetComponent<TileDoor>(); //���������� ������ � �������
                    tilesDoor[px, py].InitializeDoor(GetComponent<TileMapArray>(), new Vector2Int(px, py), "TileDoor"); //��������� ���������� ������
                    break;
                }
            //case (int)TileMapType.devices: //�������
            //    {
            //        Debug.Log($"������� �������: {c_object}, {c_object.name} [{px};{py}]");
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


        ////�������� ������������ ������ ����
        //Color32 testColor = new Color32();
        //int testCount = 0;
        //int colValue = 100;
        //foreach (TilePipeNetwork pipes in pipesNetwork)
        //{
        //    //����� ������
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

        //    string result = $"��������� ��� {pipes.key}: ";
        //    foreach (Vector2Int pipe in pipes.GetEndingPipesTrueList())
        //    {
        //        result += $"[{pipe}], ";
        //    }
        //    result += $"\n ������ ���� ���������: ";
        //    foreach (Vector2Int pipe in pipes.GetTrueList())
        //    {
        //        result += $"[{pipe}], ";
        //    }
        //    Debug.Log(result);
        //}
    }
}

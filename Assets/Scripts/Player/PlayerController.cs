using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    // ��������� � �������
    ProjectInitializer projectInitializer;
    TileMapArray tilesArray;

    //���������� ������������ ����������
    Camera mainCamera;

    // ��������� � ������� ������
    PlayerMovement playerMovement;
    bool isInitialized = false;

    [Header("������� ��� ��������")]
    public RuleTile ruleTileLatice;
    public RuleTile ruleTileWall;

    LeftClickMode LCMode = LeftClickMode.None;

    /// <summary>
    /// ������ ��������� �� ������� �� ����� ������ ����
    /// </summary>
    public enum LeftClickMode
    {
        None,
        Interact,
        Disassembly,
        Assembly,
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
        //���� �� ����������� � ��������� ��������� �� ������ � ������� ������ ������ �� ��������� ���������, �� ������ ���� ���� ��� ����� ���������
        tilesArray = FindObjectOfType<TileMapArray>().GetComponent<TileMapArray>(); //!!!!!!��������� ��������!!!!!!!
        projectInitializer = FindObjectOfType<ProjectInitializer>().GetComponent<ProjectInitializer>(); //!!!!!!��������� ��������!!!!!!!
    }

    float tick_curr_time; //������� �����

    private void Update()
    {
        if (isInitialized)
        {
            //������ ������� �� ��� � ���
            tick_curr_time -= Time.deltaTime; // �������� ����� �����
            if (tick_curr_time <= 0)
            {
                tick_curr_time = ProjectInitializer.tick_time;
                ButtonsFunctions();
            }
        }
    }

    void CheckTileArray()
    {
        //����� ����� ��� ������� ���� ����
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            Debug.Log($"������ ������� {tileMap.name}, ��� ������: {tileMap.bounds}, max: {tileMap.bounds.max}, min: {tileMap.bounds.min}, ������ {tileMap.bounds.size}");

            if (transform.position.x >= tileMap.bounds.xMin && transform.position.x <= tileMap.bounds.xMax &&
                transform.position.y >= tileMap.bounds.yMin && transform.position.y <= tileMap.bounds.yMax)
            {
                tilesArray = tileMap;
                Debug.Log($"������ {name} ��������� �� {tileMap.name}");
            }
        }

    }

    /// <summary>
    /// ���������� ������� �� ������� ����. ��� ������.
    /// </summary>
    private void ButtonsFunctions()
    {
        //����
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

        //���
        if (Input.GetMouseButton(0))
        {
            //��������� ��������� � �������
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
                            Debug.Log($"����������� ����� �� ���: {LCMode}");
                            //tilesArray.TileRemove(clickTileArrayPosition, (int)TileMapArray.TileMapType.blocks);
                            break;
                        }
                    case LeftClickMode.Interact:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������������� � ������ [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                                try
                                {
                                    tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].ChangeState();
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"����� �� ������ �� ����� ��� ���������� {clickTilePlacePosition}\n������: {e}");
                                }
                            break;
                        }
                    case LeftClickMode.Create:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������� �� ����� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");
                            //Debug.Log($"�������� ��������� ����� TileSet: [{clickTileArrayPosition.x}, {clickTileArrayPosition.y}], Tile: [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}], World: [{clickWorldPosition.x}, {clickCellPosition.y}], cell: [{clickCellPosition.x}, {clickWorldPosition.y}]");

                            //������������� ����
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
                    case LeftClickMode.Disassembly:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������� ���� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //��������� ����
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
                            } //��� ������� ���
                            else if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.floor;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            }
                            break;
                        }
                    case LeftClickMode.Assembly:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n ������ ���� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //�������� ����
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].Assamble();
                                if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToComplete)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.blocks;
                                    Debug.Log($"�������� ����� ��: [clickTilePlacePosition:{clickTilePlacePosition}, clickTileArrayPosition:{clickTileArrayPosition}]");
                                    Destroy(tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].gameObject);
                                    CreateWall(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                                }
                            }

                            //�������� �������� ���� ����������� ��������, ������ ��� � ������ � ������� ��� ����������� ������, ����� ���� ����������
                            if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] == null)
                            {
                                CreateWall(clickTilePlacePosition, clickTileArrayPosition, ruleTileWall);
                            }
                            break;
                        }
                    case LeftClickMode.AddGas:
                        {
                            if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                                try
                                {
                                    Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n��������� ��� �� �����: {tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].name}");
                                    tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].UpdateGas(500f, 0f); //��������� ��������
                                                                                                                         //tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].DeactivateBlockGas(); //������������ ����
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"����� �� ������ �� ����� ��� ���������� {clickTilePlacePosition}\n������: {e}");
                                }


                            break;
                        }
                    default:
                        {
                            Debug.Log($"����������� ����� �� ���");
                            break;
                        }
                }
            }
        }

        //���
        if (Input.GetMouseButtonDown(1))
        {
            //�������� �������
            Vector3 clickWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickTilePosition = new Vector3Int((int)(tilesArray.transform.position.x + clickWorldPosition.x + Mathf.Abs(tilesArray.bounds.xMin)),
                                                          (int)(tilesArray.transform.position.y + clickWorldPosition.y + Mathf.Abs(tilesArray.bounds.yMin)), 0);
            Vector3Int clickCellPosition = tilesArray.map[0].WorldToCell(Input.mousePosition);
            Vector3Int clickTileSetPosition = new Vector3Int((int)(tilesArray.transform.position.x + clickTilePosition.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                             (int)(tilesArray.transform.position.y + clickTilePosition.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);

            //���������� � �����
            if (CheckInArrayBounds(clickTileSetPosition))
            {
                string strPosition = $"WorldPosition: {clickWorldPosition}, CellPosition: {clickCellPosition}";
                string strGas = tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null ? $"� {tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y]} {tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].tilePlace}" : "����� ���";
                string strPressure = tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null ? $"{tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].pressure}" : "�������� ���";
                Debug.Log($"Tile {clickTilePosition}, �������� = {strPressure}, ����������� �����: {strGas} \n{strPosition}");
            }
        }
    }

    private void CreateWall(Vector3Int clickTilePlacePosition, Vector3Int clickTileArrayPosition, RuleTile ruleTileWall)
    {
        int c_map = (int)TileMapArray.TileMapType.blocks;
        tilesArray.map[c_map].SetTile(clickTileArrayPosition, ruleTileWall);
        tilesArray.TileAdd(clickTilePlacePosition, c_map);
        tilesArray.map[c_map].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //��������� ������� ���������

        //��������� ��� ��� ������ ���� �� ����
        if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.activeInHierarchy == true)
        {
            tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.SetActive(false);
        }
    }

    //���� ��� ��������� �� �������
    public void DisassemblyMode()
    {
        LCMode = LeftClickMode.Disassembly;
        Debug.Log($"������� ����� ����� ������ ���� ������: {LCMode}");
    }
    public void AssemblyMode()
    {
        LCMode = LeftClickMode.Assembly;
        Debug.Log($"������� ����� ����� ������ ���� ������: {LCMode}");
    }
    public void CreateMode()
    {
        LCMode = LeftClickMode.Create;
        Debug.Log($"������� ����� ����� ������ ���� ������: {LCMode}");
    }
    public void InteractMode()
    {
        LCMode = LeftClickMode.Interact;
        Debug.Log($"������� ����� ����� ������ ���� ������: {LCMode}");
    }
    public void AddGasMode()
    {
        LCMode = LeftClickMode.AddGas;
        Debug.Log($"������� ����� ����� ������ ���� ������: {LCMode}");
    }
    public void DefaultMode()
    {
        LCMode = LeftClickMode.None;
        Debug.Log($"������� ����� ����� ������ ���� ������: {LCMode}");
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

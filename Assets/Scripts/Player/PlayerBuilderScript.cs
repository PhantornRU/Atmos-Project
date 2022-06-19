using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerBuilderScript : MonoBehaviour
{
    [Header("������� ��� ��������")]
    public RuleTile ruleTileLatice;
    public RuleTile ruleTileWall;
    public RuleTile ruleTileWindow;
    public GameObject gameObjectDoor;

    [Header("������� ������ � ����")]
    public LeftClickMode LCMode = LeftClickMode.None;
    public BuildType current_build_type = BuildType.None;
    TileBlock.BuildingType curBuildType = TileBlock.BuildingType.None;
    public bool isDebugMode = true;

    //���������� ������������ ����������
    Camera mainCamera;
    float radiusCheckPlayerMouse = 100f;
    [HideInInspector] public TileMapArray tilesArray;
    PlayerController playerController;
    PlayerInterfaceScript playerInterface;

    /// <summary>
    /// ������ ��������� �� ������� �� ����� ������ ����
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
    /// ���������� ������� �� ������� ����.
    /// </summary>
    public void ButtonsFunctions()
    {
        //����
        if (Input.GetKey(KeyCode.Space))
        {
            //�������� ���� ������
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

            if ((CheckMouseInRadius(radiusCheckPlayerMouse) || isDebugMode) && CheckInArrayBounds(clickTileArrayPosition))
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
                    case LeftClickMode.DebugCreate:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������� �� ����� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            if (current_build_type != BuildType.None)
                            {
                                //������������� ����
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
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������� �� ����� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //��������� ����
                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.doors;
                                //Debug.Log($"������� ����� {tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].name} �� �����������: [clickTilePlacePosition: {clickTilePlacePosition}, clickTileArrayPosition{clickTileArrayPosition}]");
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            }
                            else if (tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.blocks;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            } //��� ������� ���
                            else if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                int c_map = (int)TileMapArray.TileMapType.floor;
                                tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            }
                            break;
                        }
                    case LeftClickMode.Disassembly:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������� ���� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //��������� ����
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
                            ////��� ������� ���
                            //else if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            //{
                            //    int c_map = (int)TileMapArray.TileMapType.floor;
                            //    tilesArray.TileRemove(clickTileArrayPosition, clickTilePlacePosition, c_map);
                            //}
                            break;
                        }
                    case LeftClickMode.Assembly:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n ������ ���� [{clickTilePlacePosition.x}, {clickTilePlacePosition.y}]");

                            //�������� ����

                            if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y] != null)
                            {
                                tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].Assamble();
                                if (tilesArray.tilesDoor[clickTilePlacePosition.x, clickTilePlacePosition.y].isNeedToComplete)
                                {
                                    int c_map = (int)TileMapArray.TileMapType.doors;
                                    Debug.Log($"�������� ����� ��: [clickTilePlacePosition:{clickTilePlacePosition}, clickTileArrayPosition:{clickTileArrayPosition}]");
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
                                    Debug.Log($"�������� ����� ��: [clickTilePlacePosition:{clickTilePlacePosition}, clickTileArrayPosition:{clickTileArrayPosition}]");
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


                            //�������� �������� ���� ����������� ��������, ������ ��� � ������ � ������� ��� ����������� ������, ����� ���� ����������
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
    /// <summary>
    /// ������� ���� �� ruleTile � ������� ��� � ������
    /// </summary>
    /// <param name="clickTilePlacePosition"></param>
    /// <param name="clickTileArrayPosition"></param>
    /// <param name="ruleTile"></param>
    private void CreateBlock(Vector3Int clickTilePlacePosition, Vector3Int clickTileArrayPosition, RuleTile ruleTile)
    {
        int c_map = (int)TileMapArray.TileMapType.blocks;
        tilesArray.map[c_map].SetTile(clickTileArrayPosition, ruleTile);
        tilesArray.TileAdd(clickTilePlacePosition, c_map);
        tilesArray.map[c_map].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //��������� ������� ���������

        //tilesArray.tilesBlock[clickTilePlacePosition.x, clickTilePlacePosition.y].Initialize(tilesArray, (Vector2Int)clickTilePlacePosition, "TileBlockCreate");

        //��������� ��� ��� ������ ���� �� ����
        if (tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.activeInHierarchy == true)
        {
            tilesArray.tilesGas[clickTilePlacePosition.x, clickTilePlacePosition.y].smokeObject.SetActive(false);
        }
    }

    /// <summary>
    /// ������� ������ ����� � ������� ��� � ������
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
    /// �������� ���������� ���� � �������� ������� � �������� �� ������ ������
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
    /// ���������� �������� �� ������� ��������� ��������
    /// </summary>
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
}

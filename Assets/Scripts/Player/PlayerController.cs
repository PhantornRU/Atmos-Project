using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    Camera mainCamera;
    Rigidbody2D rb;
    float horizontal, vertical;
    TileMapArray tilesArray;
    ProjectInitializer projectInitializer;

    Vector2 moveVector;

    [SerializeField]
    float speed = 1000;

    public Sprite[] sprites = new Sprite[4];

    bool isInitialized = false;

    [Header("�������� �������")]
    public RuleTile ruleTileLatice;
    public RuleTile ruleTileWall;

    LeftClickMode LCMode = LeftClickMode.None;

    /// <summary>
    /// ������ ��������� �� ������� �� ����� ������ ����
    /// </summary>
    private enum LeftClickMode
    {
        None,
        Interact,
        Damage,
        Create,
        AddGas
    }

    public void Initialize()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        CheckTileArray();

        isInitialized = true;
    }

    private void Start()
    {
        //���� �� ����������� � ��������� ��������� �� ������ � ������� ������ ������ �� ��������� ���������, �� ������ ���� ���� ��� ����� ���������
        tilesArray = FindObjectOfType<TileMapArray>().GetComponent<TileMapArray>(); //!!!!!!��������� ��������!!!!!!!
        projectInitializer = FindObjectOfType<ProjectInitializer>().GetComponent<ProjectInitializer>(); //!!!!!!��������� ��������!!!!!!!
    }

    float tick_curr_time; //������� �����

    private void FixedUpdate()
    {
        //���������� �������� ���� ������ ��� ���������
        if (isInitialized)
        {
            ChangeSprite();

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            moveVector = new Vector2(horizontal, vertical);
            rb.AddForce(moveVector * speed * rb.mass * Time.deltaTime, ForceMode2D.Force);

            mainCamera.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -10);
        }

        //������ ������� �� ��� � ���
        tick_curr_time -= Time.deltaTime; // �������� ����� �����
        if (tick_curr_time <= 0)
        {
            tick_curr_time = ProjectInitializer.tick_time;
            ButtonsFunctions();
        }
    }

    void ChangeSprite()
    {
        if (horizontal > 0 && horizontal > vertical)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[2];
        }
        if (horizontal < 0 && horizontal < vertical)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[3];
        }
        if (vertical > 0 && vertical > horizontal)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[1];
        }
        if (vertical < 0 && vertical < horizontal)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
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
            Vector3Int clickTilePosition = new Vector3Int((int)(tilesArray.transform.position.x + clickWorldPosition.x + Mathf.Abs(tilesArray.bounds.xMin)), 
                                                          (int)(tilesArray.transform.position.y + clickWorldPosition.y + Mathf.Abs(tilesArray.bounds.yMin)), 0);
            Vector3Int clickCellPosition = tilesArray.map[0].WorldToCell(Input.mousePosition);
            Vector3Int clickTileSetPosition = new Vector3Int((int)(tilesArray.transform.position.x + clickTilePosition.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                             (int)(tilesArray.transform.position.y + clickTilePosition.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);

            if (CheckInArrayBounds(clickTileSetPosition))
            {
                switch (LCMode)
                {
                    case LeftClickMode.None:
                        {
                            Debug.Log($"����������� ����� �� ���: {LCMode}");
                            break;
                        }
                    case LeftClickMode.Interact:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������������� � ������ [{clickTilePosition.x}, {clickTilePosition.y}]");

                            if (tilesArray.tilesDoor[clickTilePosition.x, clickTilePosition.y] != null)
                                try
                                {
                                    tilesArray.tilesDoor[clickTilePosition.x, clickTilePosition.y].ChangeState();
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"����� �� ������ �� ����� ��� ���������� {clickTilePosition}\n������: {e}");
                                }

                            break;
                        }
                    case LeftClickMode.Create:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n �������� �� ����� [{clickTilePosition.x}, {clickTilePosition.y}]");
                            //Debug.Log($"�������� ��������� ����� TileSet: [{clickTileSetPosition.x}, {clickTileSetPosition.y}], Tile: [{clickTilePosition.x}, {clickTilePosition.y}], World: [{clickWorldPosition.x}, {clickCellPosition.y}], cell: [{clickCellPosition.x}, {clickWorldPosition.y}]");

                            //������������� ����
                            if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] == null)
                            {
                                int c_map = 0;
                                tilesArray.map[c_map].SetTile(clickTileSetPosition, ruleTileLatice);
                                tilesArray.TileAdd(clickTilePosition, c_map);


                            }
                            else if (tilesArray.tilesBlock[clickTilePosition.x, clickTilePosition.y] == null)
                            {
                                int c_map = 1;
                                tilesArray.map[c_map].SetTile(clickTileSetPosition, ruleTileWall);
                                tilesArray.TileAdd(clickTilePosition, c_map);

                                tilesArray.map[c_map].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //��������� ������� ���������

                                //��������� ��� ��� ������ ���� �� ����
                                if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.activeInHierarchy == true)
                                {
                                    tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.SetActive(false);
                                }
                            }
                            break;
                        }
                    case LeftClickMode.Damage:
                        {
                            Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n ������� ���� �� ����� [{clickTilePosition.x}, {clickTilePosition.y}]");

                            //������� ����
                            if (tilesArray.tilesBlock[clickTilePosition.x, clickTilePosition.y] != null)
                            {
                                tilesArray.map[1].SetTile(clickTileSetPosition, null); //�������� ����� ����� � ��� GameObject
                                tilesArray.map[1].GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); //��������� ������� ���������

                                //�������� ��� ��� ������ ���� �� ����
                                if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.activeInHierarchy == false)
                                {
                                    tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].smokeObject.SetActive(true);
                                }
                            }
                            else if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null)
                            {
                                //tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].UpdatePressure(0); //��������� ����� ���������� ����� ����������
                                //tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].PressureTransmission(tilesArray.tick_time);
                                tilesArray.map[0].SetTile(clickTileSetPosition, null); //�������� ����� ���� � ��� GameObject
                            }


                            break;
                        }
                    case LeftClickMode.AddGas:
                        {
                            if (tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y] != null)
                                try
                                {
                                    Debug.Log($"����������� �������� ������ �� ���: {LCMode} \n��������� ��� �� �����: {tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].name}");
                                    tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].UpdatePressure(500f); //��������� ��������
                                                                                                                         //tilesArray.tilesGas[clickTilePosition.x, clickTilePosition.y].DeactivateBlockGas(); //������������ ����
                                }
                                catch (InvalidCastException e)
                                {
                                    Debug.Log($"����� �� ������ �� ����� ��� ���������� {clickTilePosition}\n������: {e}");
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

    //���� ��� ��������� �� �������
    public void DeleteMode()
    {
        LCMode = LeftClickMode.Damage;
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

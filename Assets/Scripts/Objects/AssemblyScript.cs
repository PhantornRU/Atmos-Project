using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class AssemblyScript : MonoBehaviour
{
    [Header("������� �������� ������� ������ ��� ��������")]
    public List<Sprite> spritesWall;
    //[SerializeField] private Sprite defaultSprite;
    public int countSprite = 0;
    private int countSpriteMax = 0;
    //public int countStateWhenTilemapChange = 1;

    SpriteRenderer spriteRenderer;

    public GameObject objectWhenEndDisassebly;

    private void Awake()
    {
        countSpriteMax = spritesWall.Count;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DisassemblyState()
    {
        //��� ������ �������, ������� ���-���� ��������
        if (countSprite == 0)
        {
            TileBlock tileDoor = GetComponent<TileDoor>();
            TileBlock tileBlock = GetComponent<TileBlock>();
            TileMapArray tilesArray = tileBlock.tilesArray;
            int c_map; 
            if (tileDoor)
            {
                c_map = (int)TileMapArray.TileMapType.doors;
            }
            else
            {
                c_map = (int)TileMapArray.TileMapType.blocks;
            }
            Vector3Int tilePosition = new Vector3Int((int)(tilesArray.transform.position.x + tileBlock.tilePlace.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                             (int)(tilesArray.transform.position.y + tileBlock.tilePlace.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);
            tilesArray.TileRemove(tilePosition, (Vector3Int)tileBlock.tilePlace, c_map);
        }

        countSprite++;
        countSprite = Mathf.Clamp(countSprite, 0, countSpriteMax);
        if (countSprite < countSpriteMax)
        {
            //Debug.Log($"{name} ������ ������ �� {countSprite}");
            spriteRenderer.sprite = spritesWall.ElementAt(countSprite);
        }
        else
        {
            //Debug.Log($"{name} ������� �� �������� �� {countSprite}");
            DisassemblyEnd();
        }
    }
    public void AssemblyState()
    {
        countSprite--;
        countSprite = Mathf.Clamp(countSprite, 0, countSpriteMax);

        //��� �������� ��������, ��������� ���-���� ��������
        if (countSprite == 0)// == countSpriteMax)
        {
            AssemblyEnd();
        }

        if (countSprite >= 0)
        {
            Debug.Log($"{name} ������ ������ �� {countSprite}");
            spriteRenderer.sprite = spritesWall.ElementAt(countSprite);
        }
    }

    public void DisassemblyEnd()
    {
        //����� ������ ���� �������� �� ��������� �����

        //������� ������ ������ ����� ������� ����� �������
        GameObject object_disassebmly = Instantiate(objectWhenEndDisassebly, transform);
        object_disassebmly.transform.parent = transform.parent;

        //���������� ���� ������
        TileBlock tileBlock = GetComponent<TileBlock>();
        tileBlock.isNeedToDestroy = true;
    }
    public void AssemblyEnd()
    {
        Debug.Log("���� ��������� �������� �� " + countSprite);

        //������� ����
        TileBlock tileBlock = GetComponent<TileBlock>();
        tileBlock.isNeedToComplete = true;
    }
}

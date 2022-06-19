using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class AssemblyScript : MonoBehaviour
{
    [Header("Спрайты меняющие текущий спрайт при разборке")]
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
        //при первом запуске, удаляем под-тайл тайлмапы
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
            //Debug.Log($"{name} меняет спрайт на {countSprite}");
            spriteRenderer.sprite = spritesWall.ElementAt(countSprite);
        }
        else
        {
            //Debug.Log($"{name} выходит из ассембля на {countSprite}");
            DisassemblyEnd();
        }
    }
    public void AssemblyState()
    {
        countSprite--;
        countSprite = Mathf.Clamp(countSprite, 0, countSpriteMax);

        //при конечном рассчете, добавляем под-тайл тайлмапы
        if (countSprite == 0)// == countSpriteMax)
        {
            AssemblyEnd();
        }

        if (countSprite >= 0)
        {
            Debug.Log($"{name} меняет спрайт на {countSprite}");
            spriteRenderer.sprite = spritesWall.ElementAt(countSprite);
        }
    }

    public void DisassemblyEnd()
    {
        //здесь должно быть разборка на составные части

        //Создаем пустой объект стены который можно толкать
        GameObject object_disassebmly = Instantiate(objectWhenEndDisassebly, transform);
        object_disassebmly.transform.parent = transform.parent;

        //уничтожаем этот объект
        TileBlock tileBlock = GetComponent<TileBlock>();
        tileBlock.isNeedToDestroy = true;
    }
    public void AssemblyEnd()
    {
        Debug.Log("Тест окончания ассембля на " + countSprite);

        //создаем тайл
        TileBlock tileBlock = GetComponent<TileBlock>();
        tileBlock.isNeedToComplete = true;
    }
}

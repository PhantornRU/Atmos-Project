using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class DisassemblyScript : MonoBehaviour
{
    [Header("—прайты мен€ющие текущий спрайт при разборке")]
    public List<Sprite> spritesWall;
    //[SerializeField] private Sprite defaultSprite;
    private int countSprite = 0;
    private int countSpriteMax = 0;

    SpriteRenderer spriteRenderer;

    public GameObject objectWhenEndDisassebly;

    private void Awake()
    {
        countSpriteMax = spritesWall.Count;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ChangeState()
    {
        countSprite++;
        if (countSprite < countSpriteMax)
        {
            //Debug.Log($"{name} мен€ет спрайт на {countSprite}");
            spriteRenderer.sprite = spritesWall.ElementAt(countSprite);

            //при достижении числа спрайта, удал€ет сам тайл
            if (countSprite == countSpriteMax - 1)
            {
                TileBlock tileBlock = GetComponent<TileBlock>();
                TileMapArray tilesArray = tileBlock.tilesArray;
                int c_map = (int)TileMapArray.TileMapType.blocks;
                Vector3Int tilePosition = new Vector3Int((int)(tilesArray.transform.position.x + tileBlock.tilePlace.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                                 (int)(tilesArray.transform.position.y + tileBlock.tilePlace.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);
                tilesArray.TileRemove(tilePosition, (Vector3Int)tileBlock.tilePlace, c_map);
            }
        }
        else
        {
            //Debug.Log($"{name} выходит из ассембл€ на {countSprite}");
            EndDisassebly();
        }
    }

    public void EndDisassebly()
    {
        //здесь должно быть разборка на составные части

        //—оздаем пустой объект стены который можно толкать
        GameObject object_disassebmly = Instantiate(objectWhenEndDisassebly, transform);
        object_disassebmly.transform.parent = transform.parent;

        //уничтожаем этот объект
        TileBlock tileBlock = GetComponent<TileBlock>();
        tileBlock.isNeedToDestroy = true;
    }
}

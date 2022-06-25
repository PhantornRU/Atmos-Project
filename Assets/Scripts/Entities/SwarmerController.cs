using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerController : MobController
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        cur_time_before_damage -= Time.deltaTime;
        if (cur_time_before_damage <= 0)
        {
            //PlayerController damageable = collision.transform.GetComponent<PlayerController>();

            if (LayerMask.LayerToName(collision.gameObject.layer) == "Player")
            {
                collision.transform.GetComponent<PlayerController>().Damage(damage);

                PlayHitAnimation(moveVector);
            }

            if (LayerMask.LayerToName(collision.gameObject.layer) == "Default" || LayerMask.LayerToName(collision.gameObject.layer) == "Blocker")
            {
                Vector3Int clickTilePlacePosition = new Vector3Int((int)(tilesArray.transform.position.x + transform.position.x + moveVector.x + Mathf.Abs(tilesArray.bounds.xMin)),
                                                              (int)(tilesArray.transform.position.y + transform.position.y + moveVector.y + Mathf.Abs(tilesArray.bounds.yMin)), 0);
                Vector3Int clickTileArrayPosition = new Vector3Int((int)(tilesArray.transform.position.x + clickTilePlacePosition.x - Mathf.Abs(tilesArray.bounds.xMin)),
                                                                 (int)(tilesArray.transform.position.y + clickTilePlacePosition.y - Mathf.Abs(tilesArray.bounds.yMin)), 0);

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
                    PlayHitAnimation(moveVector);
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
                    PlayHitAnimation(moveVector);
                }
                
            }

            //TileBlock tileBlock = collision.transform.GetComponent<TileBlock>();

            //Debug.Log(name + " столкнулся с коллизией леера:" + collision.gameObject.layer + " другой же " + LayerMask.NameToLayer("Blocker"));

            ////if (LayerMask.LayerToName(collision.gameObject.layer) == "Blocker")
            //if (tileBlock != null)
            //{
            //    Debug.Log(name + "столкнулся с " + collision.transform.name);
            //    PlayHitAnimation(moveVector);
            //    //pathfinding.SetLookPositionToTarget(collision.transform.position);
            //}

            cur_time_before_damage = timeSecondsBeforeDamageAgain;
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[SelectionBase]
public class TileBlock : TileObject
{
    [Header("�������� ������")]
    public bool isBlockGas = true;
    public GameObject objectWhenDisassembly;

    /// <summary>
    /// ������������� ����� �����������
    /// </summary>
    /// <param name="_tilesArray"></param>
    /// <param name="_tilePlace"></param>
    /// <param name="_name"></param>
    public void Initialize(TileMapArray _tilesArray, Vector2Int _tilePlace, string _name)
    {
        tilesArray = _tilesArray; //������ ������ �� ������� ������
        tilePlace = _tilePlace; //������� �������

        name = $"{_name}{tilePlace}";

        //������ ��������� ��� ������ ���� ��� ��� ���� ����
        ActivateBlockGas(); //���������� ����������� ����
    }

    [HideInInspector] public bool isNeedToDestroy = false;

    public void Dissamble()
    {
        if (objectWhenDisassembly != null)
        {
            GameObject object_disassebmly = Instantiate(objectWhenDisassembly, transform);
            object_disassebmly.transform.parent = transform.parent;
            TileBlock tileBlock = object_disassebmly.GetComponent<TileBlock>();
            tileBlock.Initialize(tilesArray, tilePlace, name);

            tilesArray.tilesBlock[tilePlace.x, tilePlace.y] = tileBlock;

            //Debug.Log("������ ������ " + object_disassebmly.name +" ��� ������� ��: " + name);
            isNeedToDestroy = true;
            //ActivateBeforeDestroyed();
        }
        else
        {
            DisassemblyScript disassembly = GetComponent<DisassemblyScript>();
            if (disassembly) // ���� �� ����, �� �������� �������� ������
            {
                //Debug.Log("������ ��������� �: " + name);
                disassembly.ChangeState();
            }
            else
            {
                Debug.Log("����������� ������ ��� ������� " + name);
            }
        }
    }

    /// <summary>
    /// ����������� ���������� ����
    /// </summary>
    public void DeactivateBlockGas()
    {
        isBlockGas = false;
        //��������� ��������� �������� �������� �� �����, ���� ��� ������������
        if (tilesArray.tilesGas[tilePlace.x, tilePlace.y] != null)
        {
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].DeactivateBlockGas();
            //tilesArray.tilesGas[tilePlace.x, tilePlace.y].isActive = true;
        }
    }

    /// <summary> 
    /// ��������� ���������� ���� 
    /// </summary>
    public void ActivateBlockGas()
    {
        isBlockGas = true;
        if (tilesArray.tilesGas[tilePlace.x, tilePlace.y] != null)
        {
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].ActivateBlockGas();
        }
    }

    public override void ActivateBeforeDestroyed()
    {
        DeactivateBlockGas();

        base.ActivateBeforeDestroyed();
    }
}

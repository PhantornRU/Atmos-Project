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

    [HideInInspector] public bool isNeedToDestroy = false;
    [HideInInspector] public bool isNeedToComplete = false;

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

    AssemblyScript assemblyScript;

    public void Diassamble()
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
            //����������� ������� ������
            if (assemblyScript == null)
            {
                assemblyScript = GetComponent<AssemblyScript>();
            }
            else // ���� �� ����, �� �������� �������� ������
            {
                //Debug.Log("������ ��������� �: " + name);
                assemblyScript.DisassemblyState();
            }
        }
    }

    public void Assamble()
    {
        //���� ������ ����� null, �� �������� �� ����������
        if (assemblyScript == null)
        {
            //assemblyScript = GetComponent<AssemblyScript>();
            Debug.Log("����������� �������, �������� �� �����������");
        }
        //���� ������ ��� �������� ����� ��������
        else
        {
            assemblyScript.AssemblyState();
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

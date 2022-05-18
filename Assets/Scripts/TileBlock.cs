using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[SelectionBase]
public class TileBlock : MonoBehaviour
{
    [Header("�������� ������")]
    [HideInInspector] public TileMapArray tilesArray;
    public Vector2Int tilePlace;
    public bool isBlockGas = true;

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
        if (tilesArray.tilesGas[tilePlace.x, tilePlace.y] != null)
        {   
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].ActivateBlockGas(); //���������� ����������� ����
            tilesArray.tilesGas[tilePlace.x, tilePlace.y].isNeedSmoke = false; //��������� �������� ����
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
}

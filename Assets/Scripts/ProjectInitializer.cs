using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectInitializer : MonoBehaviour
{
    public float tick_time = 0.1f; //����� � �������� ��� �������

    void Start()
    {
        //������������� �������� �� ������������� ������� ��� �� ���������� ���������

        //�������������� ���� ���� ��� �� ���������� ���������
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            tileMap.Initialize(tick_time);
        }

        //�������������� ������ � ��� ���������� ����� ���� �����
        FindObjectOfType<PlayerController>().Initialize();
    }
}

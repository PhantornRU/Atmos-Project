using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectInitializer : MonoBehaviour
{
    public float tick_time = 0.1f; //����� � �������� ��� �������
    [SerializeField] float tick_curr_time; //������� �����

    private List<TileMapArray> tileMapArray;
    private List<DevicesManager> devicesManager;

    void Start()
    {
        tileMapArray = new List<TileMapArray>();
        devicesManager = new List<DevicesManager>();

        //������� ��� ������ ���������
        BoundsInt bounds = new BoundsInt();

        //������������� �������� �� ������������� ������� ��� �� ���������� ���������

        //�������������� �������� ��� �� ���������� ���������
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            tileMap.Initialize(tick_time);
            tileMapArray.Add(tileMap);

            bounds = tileMap.bounds; //!!! ��������, ���� �� ����� ����������� �������������� �������� !!!
        }

        //�������������� �������
        foreach (DevicesManager devices in FindObjectsOfType<DevicesManager>())
        {
            devices.Initialize(tick_time, bounds);
            devicesManager.Add(devices);
        }

        //�������������� ������ � ��� ���������� ����� ���� �����
        FindObjectOfType<PlayerController>().Initialize();
    }

    public bool isNeedUpdateArray = true;//= false; !!!��������� ��������� ��������!!! //��������, ���������� �� ���������� ��� ����� �� ��������� ����� UpdateArray()

    //��������� ���������
    void FixedUpdate()
    {
        tick_curr_time -= Time.deltaTime; // �������� ����� �����
        if (tick_curr_time <= 0) //����� �����
        {

            foreach (TileMapArray tileMap in tileMapArray)
            {
                tileMap.UpdateArray();
                //isNeedUpdateArray = tileMap.countActivate > 0; //isNeedUpdateArray = tileMap.countActivate > 0 ? true : false;
            }

            //��������� �������
            if (isNeedUpdateArray)
            {
                foreach (DevicesManager devices in devicesManager)
                {
                    devices.UpdateDevices();
                }
            }

            tick_curr_time = tick_time; // ��������� ������ �������
            //isNeedUpdateArray = true;
        }

    }
}

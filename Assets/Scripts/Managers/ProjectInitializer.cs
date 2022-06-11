using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectInitializer : MonoBehaviour
{
    public const float tick_time = 0.1f; //����� � �������� ��� �������
    [SerializeField] float tick_curr_time; //������� �����

    private List<TileMapArray> tileMapArray;
    private List<DevicesManager> devicesManager;

    public bool isNeedAtmosUpdate = true;

    void Start()
    {
        tileMapArray = new List<TileMapArray>();
        devicesManager = new List<DevicesManager>();

        //������� ��� ������ ���������
        BoundsInt bounds = new BoundsInt();

        //������������� �������� �� ������������� ������� ��� �� ���������� ���������

        //�������������� �������
        foreach (DevicesManager devices in FindObjectsOfType<DevicesManager>())
        {
            devices.Initialize(tick_time, bounds);
            devicesManager.Add(devices);
        }

        //�������������� �������� ��� �� ���������� ���������
        foreach (TileMapArray tileMap in FindObjectsOfType<TileMapArray>())
        {
            tileMap.Initialize(tick_time);
            tileMapArray.Add(tileMap);

            bounds = tileMap.bounds; //!!! ��������, ���� �� ����� ����������� �������������� �������� !!!
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
            if (isNeedAtmosUpdate)
            {
                foreach (TileMapArray tileMap in tileMapArray)
                {
                    tileMap.UpdateTileMaps();
                    //isNeedUpdateArray = tileMap.countActivate > 0; //isNeedUpdateArray = tileMap.countActivate > 0 ? true : false;
                }

                //��������� �������
                //!!! �������� �������� ��-�� ������������ !!!
                //if (isNeedUpdateArray)
                //{
                //    foreach (DevicesManager devices in devicesManager)
                //    {
                //        devices.UpdateDevices();
                //    }
                //}

                tick_curr_time = tick_time; // ��������� ������ �������
                                            //isNeedUpdateArray = true;
            }
        }

    }
}

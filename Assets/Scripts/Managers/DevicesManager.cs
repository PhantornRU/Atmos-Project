using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DevicesManager : MonoBehaviour
{
    //������-�������� �� �����, ��� ��� ������ ������ ����� ���������������� ������� Start/Awake(�������� �� ��� ��� ������ ��������)
    //� ����� ���������� �� ����������� ����������� ������ �� ���.

    public List<Tilemap> atmosphereTileMapDevices; //������ ���� ������� ��������-�������� �����������
    public List<Tilemap> usingTileMapDevices; //������ ���� ������� ��������-�������� � �������� ����� �����������������

    public List<AtmosDevice> listAtmosDevices = new List<AtmosDevice>();

    //private bool isInitializeCompleted = false; //�������� �� ���������� �������������

    public void Initialize(float _tick_time, BoundsInt bounds)
    {
        DevicesInitialize(bounds);

        //isInitializeCompleted = true;

    }

    private void DevicesInitialize(BoundsInt bounds)
    {
        //�������������� ��� ������� � �������� ����� �����������������
        foreach (Tilemap tilemap_devices in usingTileMapDevices)
        {
            foreach (Transform devices in tilemap_devices.transform)
            {
                devices.GetComponent<DeviceObject>().Initialize(GetTilePosition(devices, bounds));
            }
        }

        //�������������� ��� ������� ������������
        foreach (Tilemap tilemap_devices in atmosphereTileMapDevices)
        {
            foreach (Transform devices in tilemap_devices.transform)
            {
                devices.GetComponent<AtmosDevice>().Initialize(GetTilePosition(devices, bounds));
                listAtmosDevices.Add(devices.GetComponent<AtmosDevice>());
                //Debug.Log($"�������� {devices.name} � {tilemap_devices}");
            }
        }
    }

    //public List<AtmosDevice> GetListAtmosDevices()
    //{
    //    List<AtmosDevice> result = new List<AtmosDevice>();
    //    foreach (Tilemap tilemap_devices in atmosphereTileMapDevices)
    //    {
    //        foreach (Transform devices in tilemap_devices.transform)
    //        {
    //            devices.GetComponent<AtmosDevice>().Initialize(GetTilePosition(devices, bounds));
    //            listAtmosDevices.Add(devices.GetComponent<AtmosDevice>());
    //        }
    //    }
    //    return result;
    //}

    private Vector2Int GetTilePosition(Transform c_object, BoundsInt bounds)
    {
        //������� ����� �� �������
        return new Vector2Int((int)(c_object.position.x + Mathf.Abs(bounds.xMin)),
                              (int)(c_object.position.y + Mathf.Abs(bounds.yMin)));
    }

    /// <summary>
    /// ���������� ������� ������ �� ��������
    /// </summary>
    public void UpdateDevices()
    {
        //��������� ����������� ������� //!!! �������� ���������, ��� ��� �� ������������ !!!
        foreach (Tilemap tilemap_devices in atmosphereTileMapDevices)
        {
            foreach (AtmosDevice devices in listAtmosDevices)
            {
                //Debug.Log("�������� ������: " + devices.name);
                devices.UpdateAtmosDevice();
            }
        }
    }
}

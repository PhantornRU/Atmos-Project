using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceObject : TileObject
{
    public bool isToggleOn = false;

    /// <summary>
    /// ����������� ������
    /// </summary>
    public void ToggleDevice()
    {
        isToggleOn = !isToggleOn;

        if (isToggleOn)
        {
            ToggleOn();
        }
        else
        {
            ToggleOff();
        }
    }

    /// <summary>
    /// �������� ������
    /// </summary>
    public virtual void ToggleOn()
    {
        Debug.Log($"{name}-������������� [{isToggleOn}] �� ���������");
    }

    /// <summary>
    /// ��������� �����
    /// </summary>
    public virtual void ToggleOff()
    {
        Debug.Log($"{name}-������������� [{isToggleOn}] �� ���������");
    }

}

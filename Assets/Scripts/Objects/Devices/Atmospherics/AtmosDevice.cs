using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosDevice : DeviceObject
{
    public TilePipeNetwork pipesNetwork; //����� ������� ���� �����������

    protected const float gasDiff = 1.5f; //������� ����� ���������� ��� ������� ���������

    public virtual void UpdateAtmosDevice()
    {   
        Debug.Log("������ ������ �� ���������");
    }
}

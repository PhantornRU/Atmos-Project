using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePipeNetwork : MonoBehaviour
{
    public int key;
    public bool[,] allPipes;
    public List<AtmosDevice> allAtmosDevices = new List<AtmosDevice>(); //����� ������������ �������� ��� �������� ��������
    public Vector2Int size;
    public float pressure = 101f;
    float molles;

    public List<Vector2Int> pipesEndingList;

    DevicesManager devicesManager;

    public void Initialize(int keyNetwork, Vector2Int _size)
    {
        size = _size;
        allPipes = new bool[size.x, size.y];

        key = keyNetwork;

        devicesManager = FindObjectOfType<DevicesManager>().GetComponent<DevicesManager>();
    }

    public void UpdatePipeNetwork()
    {
        //��������� �������
        if (allAtmosDevices.Count > 0)
        {
            Debug.Log($"���������� ������� ���� [{key}], ����� ��������: {allAtmosDevices.Count}");
            //foreach (Vector2Int pipeEnd in PipesEndingList)
            //{
            //    Debug.Log($"� ������� ���� [{key}] - �������� ������ {pipeEnd}");
            //}
            foreach (AtmosDevice device in allAtmosDevices)
            {
                Debug.Log($"� ������� ���� [{key}] - �������� ������ {device.transform.name}");
            }
        }
        else
        {
            Debug.Log($"� ������� ���� [{key}] - ����������� ��������� �������, ����� ��������: {allAtmosDevices.Count}");
        }
    }

    public void ChangePressureGas()
    {

    }

    /// <summary>
    /// ��������� ���� � ������
    /// </summary>
    public void AddPipe(Vector2Int pipe)
    {
        allPipes[pipe.x, pipe.y] = true;
    }

    /// <summary>
    /// ��������� ������������ �� � ������� ������
    /// </summary>
    public bool CheckPipesAround(Vector2Int pipe)
    {
        bool result = false;
        for (int px = -1; px <= 1; px++)
        {
            for (int py = -1; py <= 1; py++)
            {
                int posX = pipe.x + px;
                int posY = pipe.y + py;
                if (posX >= 0 && posY >= 0 //�� �������� �������������� ����������
                    && (Mathf.Abs(px) != Mathf.Abs(py))  //��������� ������������ ��������
                    && allPipes[posX, posY]) //������ true
                {
                    //Debug.Log($"������� �����������[{pipeX}; {pipeY}] � [{posX};{posY}]");
                    result = true;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// ����������� ���� ������ � �� ��������
    /// </summary>
    public void MergeNetwork(TilePipeNetwork networkForMerge)
    {
        //Debug.Log($"[{key}] ��������� � �����: [{networkForMerge.key}]");
        foreach (Vector2Int pipe in networkForMerge.GetTrueList())
        {
            allPipes[pipe.x, pipe.y] = networkForMerge.allPipes[pipe.x, pipe.y];
            networkForMerge.allPipes[pipe.x, pipe.y] = false;
        }
    }

    /// <summary>
    /// ���������� ������� �� ��������� ������ ��� �������� �����
    /// </summary>
    public void SplitNetwork(Vector2Int splitPipePosition)
    {
        //��������� �� ������� ����, ����� ���� �� ����� �� ��� ������ ��� ��������� �� "x"(?)
    }

    /// <summary>
    /// ��������� ���� ������ � ���� ����
    /// </summary>
    public string GetInfo()
    {
        string textInfo = "���� ����: ";
        for (int px = 0; px < size.x; px++)
        {
            textInfo += "\n";
            for (int py = 0; py < size.y; py++)
            {
                string t = allPipes[px, py] ? "�" : "F";
                textInfo += $"[{px};{py}]:{t}, ";
            }
        }
        return textInfo;
    }

    /// <summary>
    /// �������� ������ �� ����� ���������� TRUE
    /// </summary>
    public List<Vector2Int> GetTrueList()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int px = 0; px < size.x; px++)
        {
            for (int py = 0; py < size.y; py++)
            {
                if (allPipes[px, py])
                {
                    result.Add(new Vector2Int(px, py));
                }
            }
        }
        return result;
    }

    public List<Vector2Int> GetEndingPipesTrueList()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        List<Vector2Int> listTrue = GetTrueList();

        foreach (Vector2Int pipe in listTrue)
        {
            int count = 0;
            for (int px = -1; px <= 1; px++)
            {
                for (int py = -1; py <= 1; py++)
                {
                    Vector2Int position = new Vector2Int(pipe.x + px, pipe.y + py);
                    if (position.x >= 0 && position.y >= 0 //�� �������� �������������� ����������
                        && (Mathf.Abs(px) != Mathf.Abs(py))  //��������� ������������ ��������
                        && allPipes[position.x, position.y]) //������ true
                    {
                        count++;
                        //Debug.Log($"������� �����������[{pipeX}; {pipeY}] � [{posX};{posY}]");
                    }
                }
            }
            if (count == 1)
            {
                result.Add(pipe);

                //������ ������ � ������ ���� �� ������������ � ���������� ��� �������
                //foreach (AtmosDevice device in devicesManager.listAtmosDevices)
                //{
                //    if (device.tilePlace == pipe)
                //    {
                //        if (!allPipesDevices.Contains(device))
                //        {
                //            allPipesDevices.Add(device);
                //            device.pipesNetwork = this;
                //        }
                //    }
                //}

            }
        }

        return result;
    }

    public void UpdateEndingPipesTrueList()
    {
        if (pipesEndingList != null)
        {
            pipesEndingList = new List<Vector2Int>();
        }

        pipesEndingList = GetEndingPipesTrueList();
        //List<AtmosDevice> devicesList = devicesManager.listAtmosDevices; //!!! ����� �������� ����� �� ��������� ������ !!!

        //������ ������ � ������ ���� �� ������������ � ���������� ��� �������
        foreach (Vector2Int pipe in pipesEndingList)
        {
            //Debug.Log("������� ����� �� " + pipe);
            foreach (AtmosDevice device in devicesManager.listAtmosDevices)
            {
                Debug.Log($"������ {device.tilePlace} ������ � {pipe}");
                if (device.tilePlace == pipe)
                {
                    Debug.Log($"��������� {device.tilePlace} ������� � {pipe}");
                    //if (allAtmosDevices.Find)
                    if (!allAtmosDevices.Contains(device))
                    //if (!allAtmosDevices.Exists(x => x == device))
                    {
                        allAtmosDevices.Add(device);
                        device.pipesNetwork = this;
                        Debug.Log($"�������� {device.tilePlace}, ������� ������� ����: {allAtmosDevices.Count}");
                    }
                    break;
                }
                else
                {
                    Debug.Log($"�� ������ {pipe} � �������, ������� ������� ����: {allAtmosDevices.Count}");
                }
            }
        }
    }
}

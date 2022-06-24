using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProjectSaveLoad : MonoBehaviour
{
    string path = "/saveFile.json";

    [Header("������� ��� �������� ��� ��������")]
    public GameObject[] loadObjects = new GameObject[11];

    /// <summary>
    /// ������ ����������:
    /// 1 - �����-����
    /// 2 - �������
    /// 3 - �������� ��������
    /// 4 - �������
    /// 5 - ����������� �����
    /// 6 - ����������� ������
    /// 7 - ����������� ����� - ����� �����
    /// 8 - ����������� ����� - ������ �����
    /// 9 - ���
    /// 10 - ������������ �����
    /// </summary>

    public void SaveAllData()
    {
        AllData allData = new AllData();
        allData.jsonDates = new List<string>();

        foreach (ISaveLoadData dataInterface in FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadData>())
        {
            allData.jsonDates.Add(dataInterface.Save());
        }

        string json = JsonUtility.ToJson(allData);
        File.WriteAllText(Application.dataPath + path, json);

        Debug.Log("���������:" + json);
    }

    /// <summary>
    /// ������ ���� �������� �� ������
    /// </summary>
    public List<string>[] loadArray = new List<string>[11];

    public void LoadAllData()
    {
        // ��������� ������
        string json = File.ReadAllText(Application.dataPath + path);
        AllData allData = JsonUtility.FromJson<AllData>(json);

        // ��������� ������ �� ������ � ��������� � ��������� ���������� ������ �� ��������
        for (int i = 0; i < loadArray.Length; i++)
        {
            loadArray[i] = new List<string>();
            foreach (string str in allData.jsonDates)
            {
                //n++;
                if (str.Contains($"\"key\":{i},"))
                {
                    //Debug.Log("������: " + str);
                    loadArray[i].Add(str);
                }
            }
        }

        // ������� ������ �� ������� � ���� �������� � �����������
        List<ISaveLoadData> listInterface = new List<ISaveLoadData>();

        foreach (ISaveLoadData dataInterface in FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadData>())
        {
            listInterface.Add(dataInterface);
        }

        // ��������� ������� ������ � ��������� ����� ������� � ����������, ���� ��� ����������� � ����
        for (int i = 0; i < loadArray.Length; i++)
        {
            // ��������� ������� � ��������� �����������
            foreach (string str in loadArray[i])
            {
                ISaveLoadData dataForDelete = null;
                foreach (ISaveLoadData dataInterface in listInterface)
                {
                    if (str.Contains($"\"key\":{dataInterface.Key},"))
                    {
                        dataInterface.Load(str);
                        dataForDelete = dataInterface;
                        //Debug.Log($"�������� {dataInterface} \n� {str}");
                        break;
                    }
                }
                //������� ��������� ��������� �� ������, ����� ������� ����� ������
                if (dataForDelete != null)
                {
                    listInterface.Remove(dataForDelete);
                }
                else
                {
                    GameObject loadObject = Instantiate(loadObjects[i]);
                    loadObject.transform.SetParent(this.transform);
                    loadObject.GetComponent<ISaveLoadData>().Load(str);
                }
            }
        }


        // ������ ��������� � �������
        string result = "";
        int m = 0;
        for (int i = 0; i < loadArray.Length; i++)
        {
            foreach (string str in loadArray[i])
            {
                m++;
                result +=  $"\n [{i};{m}]: {str}";
            }
        }

        Debug.Log("���������: " + result);


        // ���������� ���������� ������� �� ����������� � ����������
        result = "";
        foreach (ISaveLoadData dataInterface in FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadData>())
        {
            result += $"\n {dataInterface}";
        }

        Debug.Log("���������� ���������� ��� ����������: " + result);
    }

    class AllData
    {
        //public int[] keys;
        public List<string> jsonDates;
    }
}

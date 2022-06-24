using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProjectSaveLoad : MonoBehaviour
{
    string path = "/saveFile.json";

    [Header("Объекты для создания при загрузке")]
    public GameObject[] loadObjects = new GameObject[11];

    /// <summary>
    /// Данные сохранения:
    /// 1 - игрок-дрон
    /// 2 - свармер
    /// 3 - оболочка свармера
    /// 4 - коробка
    /// 5 - разобранная стена
    /// 6 - разобранное стекло
    /// 7 - разобранная дверь - левая часть
    /// 8 - разобранная дверь - правая часть
    /// 9 - мяч
    /// 10 - динамическая лампа
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

        Debug.Log("Сохранено:" + json);
    }

    /// <summary>
    /// Список всех объектов по ключам
    /// </summary>
    public List<string>[] loadArray = new List<string>[11];

    public void LoadAllData()
    {
        // Загружаем данные
        string json = File.ReadAllText(Application.dataPath + path);
        AllData allData = JsonUtility.FromJson<AllData>(json);

        // Сортируем данные по ключам и добавляем в отдельный внутренний массив со списками
        for (int i = 0; i < loadArray.Length; i++)
        {
            loadArray[i] = new List<string>();
            foreach (string str in allData.jsonDates)
            {
                //n++;
                if (str.Contains($"\"key\":{i},"))
                {
                    //Debug.Log("Найден: " + str);
                    loadArray[i].Add(str);
                }
            }
        }

        // Создаем список из текущих в игре объектов с интерфейсом
        List<ISaveLoadData> listInterface = new List<ISaveLoadData>();

        foreach (ISaveLoadData dataInterface in FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadData>())
        {
            listInterface.Add(dataInterface);
        }

        // Обновляем текущие данные и добавляем новые объекты с сохранения, если они отсутствуют в игре
        for (int i = 0; i < loadArray.Length; i++)
        {
            // Обновляем объекты и добавляем сохраненные
            foreach (string str in loadArray[i])
            {
                ISaveLoadData dataForDelete = null;
                foreach (ISaveLoadData dataInterface in listInterface)
                {
                    if (str.Contains($"\"key\":{dataInterface.Key},"))
                    {
                        dataInterface.Load(str);
                        dataForDelete = dataInterface;
                        //Debug.Log($"Сравнены {dataInterface} \nи {str}");
                        break;
                    }
                }
                //Убираем найденный интерфейс из списка, иначе создаем новый объект
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


        // Выдаем результат в консоль
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

        Debug.Log("Загружено: " + result);


        // Отображаем оставшиеся объекты не относящиеся к сохранению
        result = "";
        foreach (ISaveLoadData dataInterface in FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadData>())
        {
            result += $"\n {dataInterface}";
        }

        Debug.Log("Оставшиеся интерфейсы при сортировке: " + result);
    }

    class AllData
    {
        //public int[] keys;
        public List<string> jsonDates;
    }
}

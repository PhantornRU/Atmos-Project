using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePipeNetwork : MonoBehaviour
{
    public int key;
    public bool[,] allPipes;
    public List<AtmosDevice> allAtmosDevices = new List<AtmosDevice>(); //будем использовано вбудущем при введении девайсов
    public Vector2Int size;
    public float pressure = 128f;
    float molles;

    public List<Vector2Int> pipesEndingList;

    DevicesManager devicesManager;
    TileMapArray tilesArray;

    public void Initialize(int keyNetwork, Vector2Int _size)
    {
        size = _size;
        allPipes = new bool[size.x, size.y];

        key = keyNetwork;

        devicesManager = FindObjectOfType<DevicesManager>().GetComponent<DevicesManager>();
        tilesArray = FindObjectOfType<TileMapArray>().GetComponent<TileMapArray>();
    }

    private float checkDifferentGas = 5.0f;

    public void UpdatePipeNetwork()
    {
        //обновляем девайсы
        if (allAtmosDevices.Count > 0)
        {
            //Debug.Log($"Обновление системы труб [{key}], число девайсов: {allAtmosDevices.Count}");
            foreach (AtmosDevice device in allAtmosDevices)
            {
                Debug.Log($"У системы труб [{key}] - обновлен девайс {device.transform.name}");
                ChangePressureGas(device);
            }
        }
        else
        {
            Debug.Log($"У системы труб [{key}] - отсутствуют доступные девайсы, число девайсов: {allAtmosDevices.Count}");
        }
    }

    /// <summary>
    /// Обновляем тайлы газа по девайсам, попутно обновляя давление в системе труб
    /// </summary>
    /// <param name="device"></param>
    private void ChangePressureGas(AtmosDevice device)
    {
        Vector2Int place = device.GetTilePosition(tilesArray.bounds);
        TileGas tile = tilesArray.tilesGas[place.x, place.y];
        float tick_time = ProjectInitializer.tick_time;
        float speedGasChange = tile.SpeedGasChange(pressure, tick_time);

        //проверяем давление
        int mark = 0;
        if (tile.pressure > pressure + checkDifferentGas)
        {
            mark = -1;
        }
        else if (tile.pressure < pressure - checkDifferentGas)
        {
            mark = 1;
        }

        //Обновляем давление
        if (mark != 0)
        {
            tile.UpdatePressure(speedGasChange * mark);
            ChangePressureNetwork(speedGasChange * mark * -1);
        }
    }
    private void ChangePressureNetwork(float _pressure)
    {
        pressure += _pressure;
    }

    /// <summary>
    /// Добавляем тайл в массив
    /// </summary>
    public void AddPipe(Vector2Int pipe)
    {
        allPipes[pipe.x, pipe.y] = true;
    }

    /// <summary>
    /// Проверяем пересекается ли с тайлами вокруг
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
                if (posX >= 0 && posY >= 0 //не являются отрицательными значениями
                    && (Mathf.Abs(px) != Mathf.Abs(py))  //исключаем диагональные значения
                    && allPipes[posX, posY]) //выдают true
                {
                    //Debug.Log($"Найдено пересечение[{pipeX}; {pipeY}] с [{posX};{posY}]");
                    result = true;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Объединение двух систем и их массивов
    /// </summary>
    public void MergeNetwork(TilePipeNetwork networkForMerge)
    {
        //Debug.Log($"[{key}] объединил с собой: [{networkForMerge.key}]");
        foreach (Vector2Int pipe in networkForMerge.GetTrueList())
        {
            allPipes[pipe.x, pipe.y] = networkForMerge.allPipes[pipe.x, pipe.y];
            networkForMerge.allPipes[pipe.x, pipe.y] = false;
        }
    }

    /// <summary>
    /// Разделение системы на несколько частей при удалении трубы
    /// </summary>
    public void SplitNetwork(Vector2Int splitPipePosition)
    {
        //указываем на делимый тайл, после чего он делит всё что больше его координат по "x"(?)
    }

    /// <summary>
    /// Получение всех данных о сети труб
    /// </summary>
    public string GetInfo()
    {
        string textInfo = "Сеть труб: ";
        for (int px = 0; px < size.x; px++)
        {
            textInfo += "\n";
            for (int py = 0; py < size.y; py++)
            {
                string t = allPipes[px, py] ? "Т" : "F";
                textInfo += $"[{px};{py}]:{t}, ";
            }
        }
        return textInfo;
    }

    /// <summary>
    /// Получаем массив со всеми значениями TRUE
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
                    if (position.x >= 0 && position.y >= 0 //не являются отрицательными значениями
                        && (Mathf.Abs(px) != Mathf.Abs(py))  //исключаем диагональные значения
                        && allPipes[position.x, position.y]) //выдают true
                    {
                        count++;
                        //Debug.Log($"Найдено пересечение[{pipeX}; {pipeY}] с [{posX};{posY}]");
                    }
                }
            }
            if (count == 1)
            {
                result.Add(pipe);

                //вносим девайс в список если он присутствует и определяем его систему
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

    public void UpdateEndingPipesTrueList(BoundsInt bounds)
    {
        if (pipesEndingList != null)
        {
            pipesEndingList = new List<Vector2Int>();
        }

        pipesEndingList = GetEndingPipesTrueList();
        //List<AtmosDevice> devicesList = devicesManager.listAtmosDevices; //!!! можно заменить чтобы не проходить заного !!!

        //вносим девайс в список если он присутствует и определяем его систему
        foreach (Vector2Int pipe in pipesEndingList)
        {
            Debug.Log("Текущая труба на " + pipe);
            foreach (AtmosDevice device in devicesManager.listAtmosDevices)
            {
                Debug.Log($"Найден {device.GetTilePosition(bounds)} вместе с {pipe}");
                if (device.GetTilePosition(bounds) == pipe)
                {
                    Debug.Log($"Сравнение {device.GetTilePosition(bounds)} успешно с {pipe}");
                    if (!allAtmosDevices.Contains(device))
                    {
                        allAtmosDevices.Add(device);
                        device.pipesNetwork = this;
                        Debug.Log($"Добавлен {device.GetTilePosition(bounds)}, текущий счетчик труб: {allAtmosDevices.Count}");
                    }
                    break;
                }
                else
                {
                    Debug.Log($"Не найден {pipe} в системе, текущий счетчик труб: {allAtmosDevices.Count}");
                }
            }
        }
    }
}

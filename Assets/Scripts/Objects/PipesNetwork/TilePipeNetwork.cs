using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePipeNetwork : MonoBehaviour
{
    public int key;
    public bool[,] allPipes;
    public List<Vector2Int> allPipesDevices; //будем использовано вбудущем при введении девайсов
    public Vector2Int size;
    public float pressure = 101f;

    public void Initialize(int keyNetwork, Vector2Int _size)
    {
        size = _size;
        allPipes = new bool[size.x, size.y];

        key = keyNetwork;
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
            }
        }

        return result;
    }


    public List<Vector2Int> PipesEndingList;

    public void UpdateEndingPipesTrueList()
    {
        if (PipesEndingList != null)
        {
            PipesEndingList = new List<Vector2Int>();
        }

        PipesEndingList = GetEndingPipesTrueList();
    }
}

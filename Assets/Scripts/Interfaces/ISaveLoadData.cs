using System.Collections;
using UnityEngine;

/// <summary>
/// Интерфейс сохранения объектов
/// </summary>
public interface ISaveLoadData
{
    public int Key { get; set; }

    /// <summary>
    /// Сохранение данных в строчный JSON формат
    /// </summary>
    /// <returns></returns>
    string Save();

    /// <summary>
    /// Загрузка данных из строчного JSON формата
    /// </summary>
    void Load(string json);

    /// <summary>
    /// Данные для преобразования в JSON
    /// </summary>
    class Data 
    { 
    }
}

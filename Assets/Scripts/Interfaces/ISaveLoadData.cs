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
    /// <param name="json"></param>
    void Load(string json);

    /// <summary>
    /// Удаляем объект в связи с его ненадобностью
    /// </summary>
    void Delete();

    /// <summary>
    /// Данные для преобразования в JSON
    /// </summary>
    class Data 
    { 
    }
}

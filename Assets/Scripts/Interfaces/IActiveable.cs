using System.Collections;
using UnityEngine;

/// <summary>
/// Интерфейс активности объекта.
/// Это общий интерфейс, где T — заполнитель
/// для типа данных, которые будут предоставлены
/// реализующим классом
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IActiveable<T>
{
    /// <summary>
    /// Установка статуса активности объекта через интерфейс
    /// </summary>
    /// <param name="isActive"></param>
    void SetActive(T isActive);
}

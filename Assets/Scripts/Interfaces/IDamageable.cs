using System.Collections;
using UnityEngine;

/// <summary>
/// Интерфейс получения урона объектом.
/// Это общий интерфейс, где T — заполнитель
/// для типа данных, которые будут предоставлены
/// реализующим классом
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDamageable<T>
{
    /// <summary>
    /// Получение урона объектом через интерфейс.
    /// </summary>
    /// <param name="damageTaken"></param>
    void Damage(T damageTaken);
}

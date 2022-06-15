using System.Collections;
using UnityEngine;

/// <summary>
/// Это общий интерфейс, где T — заполнитель
/// для типа данных, которые будут предоставлены
/// реализующим классом
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDamageable<T>
{
    void Damage(T damageTaken);
}

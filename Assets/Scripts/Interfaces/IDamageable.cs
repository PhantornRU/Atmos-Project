using System.Collections;
using UnityEngine;

/// <summary>
/// ��������� ��������� ����� ��������.
/// ��� ����� ���������, ��� T � �����������
/// ��� ���� ������, ������� ����� �������������
/// ����������� �������
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDamageable<T>
{
    /// <summary>
    /// ��������� ����� �������� ����� ���������.
    /// </summary>
    /// <param name="damageTaken"></param>
    void Damage(T damageTaken);
}

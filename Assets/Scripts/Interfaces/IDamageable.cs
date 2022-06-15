using System.Collections;
using UnityEngine;

/// <summary>
/// ��� ����� ���������, ��� T � �����������
/// ��� ���� ������, ������� ����� �������������
/// ����������� �������
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDamageable<T>
{
    void Damage(T damageTaken);
}

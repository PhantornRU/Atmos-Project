using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentController : AtmosDevice
{
    VentAnimator ventAnimator;

    public VentType currentType = VentType.filter;

    public override void Initialize(Vector2Int _tilePlace)
    {
        Debug.Log($"{name} ���������");
        InitializeTilePlace(_tilePlace);

        ventAnimator = GetComponent<VentAnimator>();
        ventAnimator.Initialize(isToggleOn, currentType, speedAnimation);
    }
    public override void UpdateAtmosDevice()
    {
        Debug.Log("������ ������ ���������");

        //if (tilesArray.tilesGas[tilePlace.x, tilePlace.y].pressure != pipesNetwork)

        //��� ��� �������� �������� ������
    }

    /// <summary>
    /// ����������� ��� ����������
    /// </summary>
    public void ToggleType()
    {
        currentType = currentType == VentType.filter ? VentType.scrubber : VentType.filter;
        ventAnimator.AnimateType(currentType, speedAnimation);
    }

    public override void ToggleOn()
    {
        ventAnimator.AnimateToggle(isToggleOn);
        Debug.Log($"{name}-������������� [{isToggleOn}] ���������");
    }

    public override void ToggleOff()
    {
        ventAnimator.AnimateToggle(isToggleOn);
        Debug.Log($"{name}-������������� [{isToggleOn}] ���������");
    }

    //private void AnimateCurrentType()
    //{
    //    ventAnimator.AnimateType(currentType);
    //}

    public enum VentType 
    {
        filter,
        scrubber
    }
}

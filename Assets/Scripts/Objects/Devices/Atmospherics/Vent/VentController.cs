using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentController : DeviceObject
{
    public TilePipeNetwork pipesNetwork;

    VentAnimator ventAnimator;
    public float speedAnimation = 1;

    public VentType currentType = VentType.filter;

    private void Start()
    {
        isToggleOn = true;
        Initialize();
    }

    public override void Initialize()
    {
        Debug.Log($"{name} ���������");

        ventAnimator = GetComponent<VentAnimator>();
        ventAnimator.Initialize(isToggleOn, currentType, speedAnimation);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosDevice : DeviceObject
{
    public TilePipeNetwork pipesNetwork;

    public virtual void UpdateAtmosDevice()
    {
        Debug.Log("Апдейт атмоса не определен");
    }
}

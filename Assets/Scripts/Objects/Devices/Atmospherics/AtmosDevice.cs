using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosDevice : DeviceObject
{
    public TilePipeNetwork pipesNetwork; //какой системе труб принадлежат

    protected const float gasDiff = 1.5f; //Разница между давлениями для запуска рассчетов

    public virtual void UpdateAtmosDevice()
    {   
        Debug.Log("Апдейт атмоса не определен");
    }
}

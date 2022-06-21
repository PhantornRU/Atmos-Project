using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobPlayerFinding : MonoBehaviour
{
    MobController controller;
    //радиус 
    public float radius = 3f;
    Vector3 targetPosition = Vector3.zero;
    bool isUpdate = false;
    //public CircleCollider2D trigger;

    private void Start()
    {
        controller = GetComponent<MobController>();
    }

    void Update()
    {
        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, GetMoveVectorToTarget(), out hit, 100f))
        //{
        //    Debug.Log(hit.transform.name);
        //}

        if (isUpdate)
        {
            controller.moveVector = GetMoveVectorToTarget();
            //Debug.Log("Направление вектора: " + GetMoveVectorToTarget());
            isUpdate = false;
        }
        //else
        //{
        //    controller.moveVector = Vector2.zero;
        //}
    }

    public void SetLookPositionToTarget(Vector3 position)
    {
        if (controller.isCanMove)
        {
            targetPosition = position;
            isUpdate = true;
            //Debug.Log($"Текущая позиция: {position}");
        } 
    }

    /// <summary>
    /// Вектор направления до цели
    /// </summary>
    /// <returns></returns>
    private Vector2 GetMoveVectorToTarget()
    {
        var heading = targetPosition - transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance; //нормализация вектора

        return direction;
    }
}

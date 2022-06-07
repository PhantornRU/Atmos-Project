using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimator : MonoBehaviour
{

    Animator animator;
    Collider2D colliderDoor;
    //private float speed;
    //private bool isOpen;

    /// <summary>
    /// ������������� �������� ����������� ��� ������
    /// </summary>
    public void Initialize(float speedAnimation)
    {
        //timer = changeTime;
        animator = GetComponent<Animator>();
        colliderDoor = GetComponent<Collider2D>();
        animator.SetFloat("speed", speedAnimation);
    }

    /// <summary>
    /// ������������ ����� � ����������� �� ���������
    /// </summary>
    public void Animate(bool isOpen, float speedAnimation)
    {
        Debug.Log($"isOpen {gameObject.name}: {isOpen}");

        animator.SetFloat("speed", speedAnimation);

        colliderDoor.enabled = !isOpen;
        animator.SetBool("isOpen", isOpen);
    }
}

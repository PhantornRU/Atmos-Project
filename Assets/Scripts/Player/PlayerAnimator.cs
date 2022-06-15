using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

class PlayerAnimator : MonoBehaviour
{
    Animator animator;

    /// <summary>
    /// Инициализация основных компонентов для работы
    /// </summary>
    public void Initialize()//(float speedAnimation)
    {
        //timer = changeTime;
        animator = GetComponent<Animator>();
        //animator.SetFloat("speed", speedAnimation);
    }

    /// <summary>
    /// Анимирование игрока в зависимости от состояния
    /// </summary>
    public void Animate(float horizontalSpeed, float verticalSpeed)
    {
        animator.SetFloat("horizontalSpeed", horizontalSpeed);
        animator.SetFloat("verticalSpeed", verticalSpeed);
    }
}

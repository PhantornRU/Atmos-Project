using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentAnimator : MonoBehaviour
{

    Animator animator;

    /// <summary>
    /// Инициализация основных компонентов для работы
    /// </summary>
    public void Initialize(bool isToggleOn, VentController.VentType type, float speedAnimation)
    {
        animator = GetComponent<Animator>();
        AnimateToggle(isToggleOn);
        if (isToggleOn)
        {
            AnimateType(type, speedAnimation);
        }
    }

    /// <summary>
    /// Анимируем переключатель
    /// </summary>
    public void AnimateToggle(bool isToggleOn)
    {
        animator.SetBool("isToggleOn", isToggleOn);
    }

    /// <summary>
    /// Анимируем текущий тип
    /// </summary>
    public void AnimateType(VentController.VentType type, float speedAnimation)
    {
        animator.SetFloat("speed", speedAnimation);

        switch (type)
        {
            case VentController.VentType.filter:
                {
                    animator.SetTrigger("TriggerFilter");
                    break;
                }
            case VentController.VentType.scrubber:
                {
                    animator.SetTrigger("TriggerScrubber");
                    break;
                }
        }
    }
}

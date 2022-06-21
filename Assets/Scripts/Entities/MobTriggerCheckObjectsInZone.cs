using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobTriggerCheckObjectsInZone : MonoBehaviour
{
    MobController controller;
    public int triggerCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<MobController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor")
        || collision.gameObject.layer == LayerMask.NameToLayer("Blocker"))
        {
            //Debug.Log("Вошел в " + collision.name);
            triggerCount++;
            if (triggerCount > 0 && !controller.isCanMove)
            {
                controller.isCanMove = true;
                controller.cur_time_cant_move = controller.timeSecondsBeforeDeathIfCantMove;

                //!!!Здесь также должна быть проверка если персонаж был вытолкан другой силой.!!!
                // Например вытолкан газом и нужно сохранять ускорение сохраняя RigidBody 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor")
        || collision.gameObject.layer == LayerMask.NameToLayer("Blocker"))
        {
            //Debug.Log("Вышел из " + collision.name);
            triggerCount--;
            if (triggerCount <= 0 && controller.isCanMove)
            {
                controller.isCanMove = false;
            }
        }
    }
}

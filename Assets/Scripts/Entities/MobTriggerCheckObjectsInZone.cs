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
            //Debug.Log("����� � " + collision.name);
            triggerCount++;
            if (triggerCount > 0 && !controller.isCanMove)
            {
                controller.isCanMove = true;
                controller.cur_time_cant_move = controller.timeSecondsBeforeDeathIfCantMove;

                //!!!����� ����� ������ ���� �������� ���� �������� ��� �������� ������ �����.!!!
                // �������� �������� ����� � ����� ��������� ��������� �������� RigidBody 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor")
        || collision.gameObject.layer == LayerMask.NameToLayer("Blocker"))
        {
            //Debug.Log("����� �� " + collision.name);
            triggerCount--;
            if (triggerCount <= 0 && controller.isCanMove)
            {
                controller.isCanMove = false;
            }
        }
    }
}

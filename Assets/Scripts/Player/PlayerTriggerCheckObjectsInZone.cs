using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerCheckObjectsInZone : MonoBehaviour
{
    PlayerMovement playerMovement;
    public int triggerCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor")
        || collision.gameObject.layer == LayerMask.NameToLayer("Blocker"))
        {
            //Debug.Log("����� � " + collision.name);
            triggerCount++;
            if (triggerCount > 0 && !playerMovement.isCanMove)
            {
                playerMovement.isCanMove = true;

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
            if (triggerCount <= 0 && playerMovement.isCanMove)
            {
                playerMovement.isCanMove = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerZone : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerController test = collision.GetComponent<PlayerController>();

        test.Damage(damage);

        Debug.Log("В зоне триггера: " + name);
    }
}

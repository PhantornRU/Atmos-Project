using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerZone : MonoBehaviour
{
    public float damage = 1f;
    public int timeSecondsBeforeDamageAgain = 2;
    float cur_time;



    private void OnTriggerStay2D(Collider2D collision)
    {
        IDamageable<float> damageable = collision.GetComponent<IDamageable<float>>();
        //PlayerController test = collision.GetComponent<PlayerController>();

        cur_time -= Time.deltaTime;
        if (cur_time <= 0 && damageable != null)
        {
            damageable.Damage(damage);
            cur_time = timeSecondsBeforeDamageAgain;
        }

        Debug.Log("В зоне триггера: " + name);
    }
}

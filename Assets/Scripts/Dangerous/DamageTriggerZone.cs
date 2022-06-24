using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerZone : MonoBehaviour, IActiveable<bool>
{
    public int damage = 1;
    public int timeSecondsBeforeDamageAgain = 2;
    float cur_time;

    public bool isActive = true;
    public void SetActive(bool _isActive)
    {
        isActive = _isActive;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isActive)
        {
            IDamageable<int> damageable = collision.GetComponent<IDamageable<int>>();
            //PlayerController test = collision.GetComponent<PlayerController>();

            cur_time -= Time.deltaTime;
            if (cur_time <= 0 && damageable != null)
            {
                damageable.Damage(damage);
                cur_time = timeSecondsBeforeDamageAgain;
            }

            //Debug.Log("В зоне триггера: " + name);
        }
    }
}

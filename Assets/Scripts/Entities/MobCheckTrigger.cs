using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobCheckTrigger : MonoBehaviour
{
    MobPlayerFinding pathfinding;
    // Start is called before the first frame update
    void Start()
    {
        pathfinding = GetComponentInParent<MobPlayerFinding>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Player")
        {
            pathfinding.SetLookPositionToTarget(collision.transform.position);
        }
    }
}

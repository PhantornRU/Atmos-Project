using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MoveScript : MonoBehaviour
{
    Rigidbody2D rigidBody;

    public float moveTime = 2.0f;
    float timerMove;

    Vector2 direction = Vector2.left;
    public float force = 0.5f;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        timerMove = moveTime;
    }

    void FixedUpdate()
    {
        if (timerMove >= 0)
        {
            timerMove -= Time.deltaTime;
            if (timerMove < 0)
            {
                timerMove = moveTime;
                if (direction.x == -1) direction.x = 1.0f; else direction.x = -1.0f;
            }
        }

        rigidBody.AddForce(direction * force);
    }
}

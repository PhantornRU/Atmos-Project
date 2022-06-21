using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerAnimator playerAnimator;

    //���������� ������������ ����������
    Camera mainCamera;
    Rigidbody2D rb;
    float horizontal, vertical;

    public Transform lightForwardTransform;

    Vector2 moveVector;

    [Header("���������")]
    public float speed = 1000;
    public bool isCanMove = true;
    bool isInitialized = false;

    public void Initialize()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        playerAnimator = GetComponent<PlayerAnimator>();
        playerAnimator.Initialize();

        isInitialized = true;
    }
    //int targetRotation = 180;
    //int currentTargetRotation = 180;
    void FixedUpdate()
    {
        //���������� �������� ���� ������ ��� ���������
        if (isInitialized)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // ������� ����� ������ ��������, ���� ����� ���������, ����� ���������� ��������� �� �������� ������� move (�������� � �����������)
            if (isCanMove)
            {
                moveVector = new Vector2(horizontal, vertical);
            }

            // ������� ������ ��������� ����������� ����� �� ������� move
            rb.AddForce(moveVector * speed * rb.mass * Time.deltaTime, ForceMode2D.Force);

            // ������������ ������ ����� �� ����������
            mainCamera.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -10);
            playerAnimator.Animate(horizontal, vertical);

            // ������� ������������� �����
            //if (horizontal != 0 || vertical != 0)
            //{
            //    lightForwardTransform.localRotation = Quaternion.Euler(0, 0, (int)(Mathf.Atan2(-horizontal, vertical) * 180 / Mathf.PI));
            //}
            Vector2 mouseInput = GetMousePosition();
            lightForwardTransform.localRotation = Quaternion.Euler(0, 0, (int)(Mathf.Atan2(-mouseInput.x, mouseInput.y) * 180 / Mathf.PI));
        }
    }

    private Vector2 GetMousePosition()
    {
        var mousePosition = Input.mousePosition;
        mousePosition.x -= Screen.width / 2;
        mousePosition.y -= Screen.height / 2;
        return mousePosition;
    }
}

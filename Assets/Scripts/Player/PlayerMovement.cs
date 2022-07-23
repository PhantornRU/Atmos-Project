using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    PlayerAnimator playerAnimator;
    PlayerController playerController;

    //внутренные используемые переменные
    Camera mainCamera;
    Rigidbody2D rb;
    float horizontal, vertical;

    public Transform lightForwardTransform;

    Vector2 moveVector;

    [Header("Параметры")]
    public float speed = 1000;
    public bool isCanMove = true;
    bool isInitialized = false;

    public void Initialize()
    {
        playerController = GetComponent<PlayerController>();

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
        //Проводимые операции если скрипт был обработан
        if (isInitialized && playerController.isActive)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // Создаем новый вектор движения, если можем двигаться, иначе продолжаем двигаться по прошлому вектору move (например в невесомости)
            if (isCanMove)
            {
                moveVector = new Vector2(horizontal, vertical);
            }

            // Толкаем нашего персонажа прилагаемой силой по вектору move
            rb.AddForce(moveVector * speed * rb.mass * Time.deltaTime, ForceMode2D.Force);

            // Передвижение камеры вслед за персонажем
            mainCamera.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -10);
            playerAnimator.Animate(horizontal, vertical);

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

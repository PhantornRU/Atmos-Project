using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IDamageable<float>
{
    [Header("��������� ������")]
    [Min(0)] public int health = 100;
    [Min(0)] public int healthMax = 100;
    // ��������� � �������
    ProjectInitializer projectInitializer;
    [HideInInspector] public TileMapArray tilesArray;

    //���������� ������������ ����������
    Camera mainCamera;

    // ��������� � ������� ������
    PlayerMovement playerMovement;
    PlayerInterfaceScript playerInterface;
    PlayerBuilderScript playerBuilder;
    bool isInitialized = false;

    public void Initialize()
    {
        mainCamera = Camera.main;

        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.Initialize();
        playerInterface = GetComponent<PlayerInterfaceScript>();
        playerBuilder = GetComponent<PlayerBuilderScript>();
        playerBuilder.Initialize(tilesArray);

        isInitialized = true;

        playerInterface.ChangeHealthUI(health, healthMax);
    }

    private void Start()
    {
        //���� �� ����������� � ��������� ��������� �� ������ � ������� ������ ������ �� ��������� ���������, �� ������ ���� ���� ��� ����� ���������
        tilesArray = FindObjectOfType<TileMapArray>().GetComponent<TileMapArray>(); //!!!!!!��������� ��������!!!!!!!
        projectInitializer = FindObjectOfType<ProjectInitializer>().GetComponent<ProjectInitializer>(); //!!!!!!��������� ��������!!!!!!!
    }

    float tick_curr_time; //������� �����

    private void Update()
    {
        if (isInitialized)
        {
            //������ ������� �� ��� � ���
            tick_curr_time -= Time.deltaTime; // �������� ����� �����
            if (tick_curr_time <= 0)
            {
                tick_curr_time = ProjectInitializer.tick_time;
                playerBuilder.ButtonsFunctions();
            }
        }
    }

    public void Damage(float damageTaken)
    {
        Debug.Log($"������ {name} ������� ����������� � ����������: {damageTaken}");

        health = Mathf.Clamp(health - (int)damageTaken, 0, healthMax);

        playerInterface.ChangeHealthUI(health, healthMax);
    }
}

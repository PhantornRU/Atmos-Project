using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IDamageable<int>, IActiveable<bool>, ISaveLoadData
{
    [Header("Параметры игрока")]
    [Min(0)] public int health = 100;
    [Min(0)] public int healthMax = 100;
    // менеджеры и скрипты
    ProjectInitializer projectInitializer;
    [HideInInspector] public TileMapArray tilesArray;

    //внутренные используемые переменные
    Camera mainCamera;
    Rigidbody2D rb;

    // параметры и скрипты игрока
    PlayerMovement playerMovement;
    PlayerInterfaceScript playerInterface;
    PlayerBuilderScript playerBuilder;
    bool isInitialized = false;

    public void Initialize()
    {
        mainCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();

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
        //ищем по координатам и проверяем находится ли объект в радиусе границ одного из доступных тайлЭррей, на случай если этих зон будет несколько
        tilesArray = FindObjectOfType<TileMapArray>().GetComponent<TileMapArray>(); //!!!!!!временная заглушка!!!!!!!
        projectInitializer = FindObjectOfType<ProjectInitializer>().GetComponent<ProjectInitializer>(); //!!!!!!временная заглушка!!!!!!!
    }

    float tick_curr_time; //Текущее время

    private void Update()
    {
        if (isInitialized && isActive)
        {
            //Методы нажатия на ЛКМ и ПКМ
            tick_curr_time -= Time.deltaTime; // Вычитаем время кадра
            if (tick_curr_time <= 0)
            {
                tick_curr_time = ProjectInitializer.tick_time;
                playerBuilder.ButtonsFunctions();
            }
        }
    }

    public bool isActive = true;
    public void SetActive(bool _isActive)
    {
        isActive = _isActive;
    }

    public void Damage(int damageTaken)
    {
        Debug.Log($"Объект {name} получит повреждение в количестве: {damageTaken}, текущее здоровье: {health}/{healthMax}");

        health -= damageTaken;
        health = Mathf.Clamp(health, 0, healthMax);

        playerInterface.ChangeHealthUI(health, healthMax);

        if (health == 0)
        {
            Death();
        }

    }

    void Death()
    {
        Debug.Log($"{name} погиб");
    }

    string Save()
    {
        return "";
    }

    void Load(string json)
    {

    }

    string ISaveLoadData.Save()
    {
        //throw new NotImplementedException();
        Data data = new Data();
        data.key = key;
        data.name = name;
        data.position = transform.localPosition;
        data.rotation = transform.localRotation;
        data.velocity = rb.velocity;
        data.angularVelocity = rb.angularVelocity;
        data.health = health;

        string result = JsonUtility.ToJson(data);

        //Debug.Log("Сохранение: " + result);

        return result;
    }

    void ISaveLoadData.Load(string json)
    {
        Data data = JsonUtility.FromJson<Data>(json);

        key = data.key;
        name = data.name;
        transform.localPosition = data.position;
        transform.localRotation = data.rotation;
        rb.velocity = data.velocity;
        rb.angularVelocity = data.angularVelocity;
        health = data.health;

        Debug.Log($"загрузка: {name}, {transform.localPosition}, {transform.localRotation}, {rb.velocity}, {rb.angularVelocity}");
    }

    [Header("Данные сохранения")]
    [Tooltip("Ключ сохранения, уникальный для объекта и != 0")]
    public int key = 0;
    public int Key { get => key; set => Key = key; }

    class Data
    {
        public int key;
        public string name;

        public Vector2 position;
        public Quaternion rotation;

        public Vector2 velocity;
        public float angularVelocity;

        public int health;
    }
}

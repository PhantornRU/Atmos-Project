using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateDynamicObjects : MonoBehaviour, ISaveLoadData
{
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

}

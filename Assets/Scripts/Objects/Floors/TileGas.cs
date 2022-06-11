using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[SelectionBase]
public class TileGas : TileObject
{
    [Header("Параметры")]
    [Tooltip("Участвуют в формулах для вычислений, PV = nRT")]
    public bool isActive = false;
    public bool isEmpty = false;
    [Space(height: 10f)]
    [Min(0)] public float pressure = 101; //P - kPa
    [Min(0)] public float concentrate = 255.0f; //n - число молей
    [Min(0)] public float temperature_K = 297.15f; //T - Температура по Кельвину, °K = °C + 273,15, Значение температуры абсолютного нуля — 0 °К, или −273,15 °C.
    [Min(0)] private float litres; //V - Объём в литрах
    const float gas_r = 8.31f; //R - Газовая постоянная, равная 8,31

    [Header("Окружные тайлы")]
    [SerializeField]private TileGas[] massGas = new TileGas[8]; //!!!!!лучше ограничить до 4-ех, а то иначе проходит по диагонали!!!!!!!!!
    [Space(height: 10f)]
    //public bool isBlockGas = false;

    [Header("Визуализирование")]
    public bool isNeedSmoke = true;
    public bool isNeedText = true;
    public GameObject smokeObject; //дым
    private SpriteRenderer smokeSprite;
    public Canvas canvasText;
    private Text textObject; //текст

    [Header("Функции для рассчетов")]
    const float gasDiff = 1.5f; //Разница между давлениями для запуска рассчетов
    public int countActive; //Счет невозможных тайлов для передачи, при достижении длины массива - деактивирует текущий тайл

    private HashSet<Rigidbody2D> affectedBodies = new HashSet<Rigidbody2D>(); //список хэша, работает быстрее чем List

    /// <summary>
    /// Инициализация тайла газа
    /// </summary>
    /// <param name="_tilesArray"></param>
    /// <param name="_tilePlace"></param>
    /// <param name="_name"></param>
    public void Initialize(TileMapArray _tilesArray, Vector2Int _tilePlace, string _name)
    {
        tilesArray = _tilesArray; //задаем ссылку на текущий массив
        tilePlace = _tilePlace; //индексы позиции
        //InitializeTilePlace(_tilePlace);
        name = $"{_name}{tilePlace}";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //добавляем в список
        if (collision.attachedRigidbody != null)
        {
            affectedBodies.Add(collision.attachedRigidbody);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //удаляем из списка
        if (collision.attachedRigidbody != null)
        {
            affectedBodies.Remove(collision.attachedRigidbody);
        }
    }
    ///<summary> 
    ///Методы передачи давления и перемещения объектов в тайл с пониженым давлением 
    ///</summary>
    public void PressureTransmission(float tick_time)
    {
        //проверяем не опустел ли наш тайл
        if (pressure <= 0)
        {
            isEmpty = true;
            countActive = 0;
            pressure = 0;
        }

        //если тайл активен и не пуст, то начинаем передавать давление на соседние тайлы
        if (isActive && !isEmpty)
        {
            countActive = 0; //счет возможных передач
            //передаем газ тайлам газов из внутреннего массива
            for (int num = 0; num < massGas.Length; num++) //для оптимизации, вместо foreach(TileGas _tileGas in massGas) - использую For, также поэтому массивы, а не списки List
            {
                if (massGas[num] != null && pressure > massGas[num].pressure + gasDiff)
                {
                    //Debug.Log($"Изменен тайл {massGas[num].tilePlace}, давление {massGas[num].pressure}");
                    float speedChange = SpeedGasChange(massGas[num].pressure, tick_time);
                    pressure -= speedChange;
                    massGas[num].UpdatePressure(speedChange);
                    countActive++;

                    PushAffectedBodies(num, tick_time, speedChange);
                }
            }
            //Вакуум. Уничтожаем газ пустыми тайлами из внутреннего массива !!!!!!НЕКОРРЕКТНО РАБОТАЕТ, НУЖНО ПЕРЕДЕЛАТЬ ПРОВЕРКИ!!!!
            for (int num = 0; num < massGas.Length; num++)
            {
                if ((massGas[num] == null) //null также относится к missing ссылки на объект.
                    && tilesArray.tilesBlock[massGas[num].tilePlace.x, tilePlace.y] == null
                    && tilesArray.tilesDoor[massGas[num].tilePlace.x, tilePlace.y] == null
                    )
                {
                    //Debug.Log($"Изменен тайл {massGas[num].tilePlace}, давление {massGas[num].pressure}");
                    //speedChange = (float)(1.414 * Mathf.Sqrt(pressure - massGas[num].pressure)) * tick_time * Time.deltaTime;
                    float speedChange = SpeedGasChange(massGas[num].pressure, tick_time);
                    pressure -= speedChange;
                    countActive++;

                    PushAffectedBodies(num, tick_time, speedChange);
                }
            }
            //деактивируем тайл газа т.к. он больше никому не передает.
            if (countActive == 0)
            {   
                isActive = false;
            }
        }
    }

    public void UpdatePressure(float _pressure)
    {
        pressure += _pressure;
        TileChanged();

        if (isEmpty && _pressure > 0)
        {
            isEmpty = false;
        }
    }

    ///<summary>  
    ///измененный тайл, вызывает методы его обновления
    ///</summary> 
    void TileChanged()
    {
        //Активируем тайл газа если он позволяет проходить газу при отсутствии блокиратора, а далее все ближайшие тайлы
        if (!CheckGasBlock(tilePlace.x, tilePlace.y))
        {
            isActive = true;
            ActivateNearTiles();
        }

        //обновляем цвет дыма и текст
        if (smokeObject.activeInHierarchy && textObject.isActiveAndEnabled)
        {
            UpdateSmoke();
            UpdateText();
        }

        //передаем тайл мапу что необходимо провести обновления тайлов
        //if (!tilesArray.isNeedUpdateArray) tilesArray.isNeedUpdateArray = true;
    }

    bool CheckGasBlock(int x, int y)
    {
        return tilesArray.tilesBlock[x, y] != null && tilesArray.tilesBlock[x, y].isBlockGas
            || tilesArray.tilesDoor[x, y] != null && tilesArray.tilesDoor[x, y].isBlockGas;
    }

    /// <summary>
    /// Формула для рассчета смещения необходимого количества газа к тайлу газа toTileNumChange, из внутреннего массива газов.
    /// </summary>
    public float SpeedGasChange(float toTilePressureChange, float tick_time)
    {
        //!!!!!здесь должна быть нормальная формула!!!!
        //speedChange = (float)(1.414 * Mathf.Sqrt(pressure - massGas[num].pressure)) * tick_time * Time.deltaTime;
        return 1.414f * Mathf.Abs(pressure - toTilePressureChange) * tick_time * Time.deltaTime;
    }

    /// <summary>
    /// Смещаем все объекты каждый заданный тик к тайлу газа toTileNumPush, из внутреннего массива газов.
    /// </summary>
    private void PushAffectedBodies(int toTileNumPush, float tick_time, float strengthPush)
    {
        //смещаем объекты сохраненные в хэше
        foreach (Rigidbody2D body in affectedBodies)
        {
            //смещение силой
            Vector2 directionToTile = ((Vector2)massGas[toTileNumPush].transform.position - body.position).normalized;
            float distance = ((Vector2)transform.position - body.position).magnitude;
            float strength = strengthPush * Mathf.Abs(pressure - massGas[toTileNumPush].pressure) * distance / (body.mass);

            float maxStrength = 100 * body.mass;
            if (strength > maxStrength) strength = maxStrength; //ограничиваем силу чтобы не вылетело за пределы

            body.AddForce(directionToTile * strength, ForceMode2D.Force); //направляем силы

            //дополнительный текст
            text2 = "dist: " + Math.Round(distance, 2).ToString();
            text3 = "str: " + Math.Round(strength, 2).ToString();

            //альтернативное смещение через Lerp 
            //body.transform.position = Vector3.Lerp(body.transform.position, massGas[num].transform.position, strength * tick_time * Time.deltaTime);
        }
    }

    /// <summary>
    /// создаем визуальный объект дыма
    /// </summary>
    public void CreateSmoke()
    {
        if (isNeedSmoke && !CheckGasBlock(tilePlace.x, tilePlace.y))
        {
            GameObject gObj = Instantiate(smokeObject, transform.position, Quaternion.identity);
            smokeObject = gObj;
            smokeObject.transform.SetParent(transform);
            //рендер спрайта
            smokeSprite = smokeObject.GetComponent<SpriteRenderer>();
            isNeedSmoke = !isNeedSmoke; //отключаем необходимость
            UpdateSmoke();
        }
    }

    /// <summary>
    /// создаем визуальный объект текста
    /// </summary>
    public void CreateText()
    {
        if (isNeedText)
        {
            //создание текста
            Canvas canvObject = Instantiate(canvasText, transform.position, Quaternion.identity);
            canvasText = canvObject;
            canvasText.transform.SetParent(transform);
            textObject = canvasText.GetComponentInChildren<Text>();
            //canvasText.enabled = true;

            //данные текста
            textObject.fontSize = 16;
            textObject.color = Color.white;
            isNeedText = !isNeedText; //отключаем необходимость
            UpdateText();
        }
    }

    ///<summary> 
    ///обновление тайла дыма и смена цвета спрайта
    ///</summary>
    private void UpdateSmoke()
    {
        if (!isNeedSmoke)
        {
            Vector3 rgbSmoke = new Vector3(); //значение для придания цвета
            float t = pressure / 2000; //определяет значение от 0 до 1, если кратно 1000
            int d1 = 10; //значение для деления нахождения диапазона между красно-желтого вектора
            int d2 = 5; //значение для деления нахождения диапазона между желто-белого вектора
            //Проверяем попадает ли под диапазон
            if (t <= 1)
            {   //от синего до красного
                rgbSmoke = Vector3.Lerp(new Vector3Int(0,0,255), new Vector3Int(255,0,0), t); // Определяет значение между векторами
            }
            else
            {
                if (t / d1 <= 1)
                {
                    //от красного до желтого
                    rgbSmoke = Vector3.Lerp(new Vector3Int(255, 0, 0), new Vector3Int(255, 255, 0), t / d1);
                }
                else
                {
                    //от желтого до белого
                    rgbSmoke = Vector3.Lerp(new Vector3Int(255, 255, 0), new Vector3Int(255, 255, 255), t / d1 / d2);
                }
            }

            //Выставляем цвета через концентрацию от синего до красного, а потом белого. !!Заменить концентрацию на температуру!!
            smokeSprite.color = new Color32(
                (byte)Mathf.Clamp(rgbSmoke.x, 0, 255),  //Red
                (byte)Mathf.Clamp(rgbSmoke.y, 0, 255),  //Green
                (byte)Mathf.Clamp(rgbSmoke.z, 0, 255),  //Blue
                (byte)Mathf.Clamp(100, 0, 255));        //Alpha
        }
    }

    string text1 = "";
    string text2 = "";
    string text3 = "";

    private void UpdateText()
    {
        if (!isNeedText)
        {
            if (isActive)
            {
                textObject.color = Color.white;
            }
            else
            {
                textObject.color = Color.black;
            }
            text1 = Math.Round(pressure, 2).ToString();
            textObject.text = $"P: {text1} kPa \n{text2}\n{text3}";
        }
    }

    ///<summary>
    ///Активация ближайших открытых тайлов
    ///</summary>
    void ActivateNearTiles()
    {
        int num = 0;
        for (int i = tilePlace.x - 1; i <= tilePlace.x + 1; i++)
        {
            for (int j = tilePlace.y - 1; j <= tilePlace.y + 1; j++)
            {
                try
                {
                    if (tilesArray.tilesGas[i, j] != null
                        && !CheckGasBlock(i, j)
                        && tilePlace != new Vector2Int(i, j))
                    {
                        if (massGas[num] == null) massGas[num] = tilesArray.tilesGas[i, j];
                        massGas[num].isActive = true;
                    }
                    else
                    {
                        if (massGas[num] != null && tilePlace != new Vector2Int(i, j)) massGas[num] = null;
                    }
                }
                catch
                {
                    continue;
                }
                num++;
            }
        }
    }

    public string ShowInfo()
    {
        return $"\nПараметры тайла {name} \nisActive: {isActive}, pressure: {pressure} tilePlace: {tilePlace}";
    }

    /// <summary>
    /// Деактивация блокировки газа
    /// </summary>
    public void DeactivateBlockGas()
    {

        isActive = true;
        TileChanged(); //запускаем обработку передачи давления на тайле
    }

    ///<summary> 
    ///активация блокировки газа 
    ///</summary>
    public void ActivateBlockGas()
    {
        isActive = false;
    }
}

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
    [Min(0)] public float pressure = 101f; //P - kPa
    [Min(0)] public float temperature_K = 297.15f; //T - Температура по Кельвину, °K = °C + 273,15, Значение температуры абсолютного нуля — 0 °К, или −273,15 °C.
    [Min(0)] public float concentrate;//22.41f; //n - концентрация частиц, количество вещества, Моли.  n=(P*V)/(R*T)
    [Min(0)] public float volume = 22.41f; //V - Объём в литрах - сколько места внутри одного тайла и сколько газа он может удерживать. 
    const float volumeTile = 2500f; //Объем вместимости тайла
    const float gas_r = 8.31f; //R - Газовая постоянная, равная 8,31

    //Формула уравнения состояния идеального газа PV = nRT, n = V/Vm, где Vm как раз = 22,41
    //стандартное давление для газов, жидкостей и твёрдых тел, равное 105 Па (100 кПа, 1 бар);
    //стандартная температура для газов, равная 293,15 К (20 °С, 32 °F);
    //стандартная молярность для растворов, равная 1 моль/л. 1 моль = 22.41 литров при одинаковом объеме
    //В 1-м кубометре = 1000 литров.

    [Header("Окружные тайлы")]
    [SerializeField]private TileGas[] massGas = new TileGas[8];
    [Space(height: 10f)]

    [Header("Визуализирование")]
    public bool isNeedSmoke = true;
    public bool isNeedText = true;
    public GameObject smokeObject; //дым
    private SpriteRenderer smokeSprite;
    public Canvas canvasText;
    public Text textObject; //текст

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

        //концентрация веществ в моллях
        concentrate = ConcentrateFormula();
    }

    /// <summary>
    /// Формула рассчета концентрации веществ, моллей // n=(P*V)/(R*T)
    /// </summary>
    float ConcentrateFormula()
    {
        return (pressure * volume) / (gas_r * temperature_K);
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
    public void TransmissionGas(float tick_time, bool isNeedVisual)
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
                    float speedPressureChange = SpeedPressureChange(massGas[num].pressure, tick_time);
                    pressure -= speedPressureChange;

                    float speedTemperatureChange = SpeedTemperatureChange(massGas[num].temperature_K, tick_time);
                    temperature_K -= speedTemperatureChange;

                    massGas[num].UpdateGas(speedPressureChange, speedTemperatureChange);
                    countActive++;

                    PushAffectedBodies(num, tick_time, speedPressureChange);

                    //Обновляем визуализаторы
                    VisualUpdate(isNeedVisual);
                }
            }
            //деактивируем тайл газа т.к. он больше никому не передает.
            if (countActive == 0)
            {   
                isActive = false;
            }
        }
    }

    public void UpdateGas(float _pressure, float _temperature_K)
    {
        pressure += _pressure;
        temperature_K += _temperature_K;
        TileChanged();

        if (isEmpty && _pressure > 0)
        {
            isEmpty = false;
        }

        if (temperature_K > 0) //если меньше абсолютного нуля, то не высчитывается
        {
            //концентрация веществ в моллях
            concentrate = ConcentrateFormula();
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
    }

    void VisualUpdate(bool isNeedVisualUpdate)
    {
        //обновляем цвет дыма и текст
        if (smokeObject.activeInHierarchy && textObject.isActiveAndEnabled)
        {
            if (isNeedVisualUpdate)
            {
                UpdateSmoke();
                UpdateText();
            }
            else
            {
                smokeObject.SetActive(false);
                textObject.gameObject.SetActive(false);
            }
        }
        else if (isNeedVisualUpdate)
        {
            smokeObject.SetActive(true);
            textObject.gameObject.SetActive(true);
            UpdateSmoke();
            UpdateText();
        }
    }

    public bool CheckGasBlock(int x, int y)
    {
        return tilesArray.tilesBlock[x, y] != null && tilesArray.tilesBlock[x, y].isBlockGas
            || tilesArray.tilesDoor[x, y] != null && tilesArray.tilesDoor[x, y].isBlockGas;
    }

    /// <summary>
    /// Формула для рассчета смещения необходимого количества газа к тайлу газа toTilePressureChange, из внутреннего массива газов.
    /// </summary>
    public float SpeedPressureChange(float toTilePressureChange, float tick_time)
    {
        return 1.414f * Mathf.Abs(pressure - toTilePressureChange) * tick_time * Time.deltaTime;
    }

    /// <summary>
    /// Формула для рассчета смещения необходимого количества газа к тайлу газа toTilePressureChange, из внутреннего массива газов.
    /// </summary>
    public float SpeedTemperatureChange(float toTileTemperatureChange, float tick_time)
    {
        return 1.414f * Mathf.Abs(temperature_K - toTileTemperatureChange) * tick_time * Time.deltaTime;
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
        if (isNeedText && !CheckGasBlock(tilePlace.x, tilePlace.y))
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

            //Выставляем цвета через концентрацию от синего до красного, а потом белого.
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
        //обновляем цвет дыма и текст
        if (smokeObject.activeInHierarchy && textObject.isActiveAndEnabled)
        {
            Debug.Log("Обновление при деактивации тайла газа у" + name);
            UpdateSmoke();
            UpdateText();
        }

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

    public override void ActivateBeforeDestroyed()
    {
        pressure = 0;
        ActivateNearTiles();
        base.ActivateBeforeDestroyed();
    }
}

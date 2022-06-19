using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakApartScript : MonoBehaviour
{
    [Min(0)] public float timeSecondsBeforeBreak = 5f;
    [Min(0)] public float current_time = 0f;

    public List<GameObject> listCreateObjectsAfterBreak;

    // Start is called before the first frame update
    void Start()
    {
        current_time = timeSecondsBeforeBreak;
    }

    // Update is called once per frame
    void Update()
    {
        current_time -= Time.deltaTime;
        if (current_time <= 0)
        {
            if (listCreateObjectsAfterBreak.Count > 0)
            {
                foreach (GameObject part in listCreateObjectsAfterBreak)
                {
                    GameObject part_object = Instantiate(part);
                    part_object.transform.parent = this.transform.parent;
                    part_object.transform.position = this.transform.position;
                }
                Destroy(this.gameObject);
            }
            else
            {
                Debug.LogWarning($"Список частей объекта пуст, удаление {name} невозможно");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobController : MonoBehaviour, IDamageable<int>
{
    [Header("Параметры сущности")]
    public int health = 30;
    public int healthMax = 30;
    public float speed = 500f;
    public bool isCanMove = true;
    Rigidbody2D rb;
    Animator animator;

    public Vector2 moveVector;

    [Header("Объект создаваемый после уничтожения")]
    public GameObject gameObjectCreateAfterDeath;

    [Header("Параметры сущности")]
    public float timeSecondsBeforeDeathIfCantMove = 10f;
    public float cur_time_cant_move = 10f;

    [Header("Нанесение урона игроку")]
    public int damage = 5;
    public int timeSecondsBeforeDamageAgain = 3;
    float cur_time_before_damage;
    public GameObject hitEffect;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        cur_time_cant_move = timeSecondsBeforeDeathIfCantMove;
    }

    // Update is called once per frame
    void Update()
    {
        // Уничтожаем текущий объект если он слишком долго не может двигаться (улетел в космос)
        if (!isCanMove)
        {
            cur_time_cant_move -= Time.deltaTime;
            if (cur_time_cant_move <= 0)
            {
                Debug.Log($"Уничтожен {name} при выходе в космос");
                Destroy(this.gameObject);
            }
        }

        animator.SetFloat("horizontal", moveVector.x);
        animator.SetFloat("vertical", moveVector.y);

        // Толкаем нашего персонажа прилагаемой силой по вектору move
        rb.AddForce(moveVector * speed * rb.mass * Time.deltaTime, ForceMode2D.Force);
    }

    public void Damage(int damageTaken)
    {
        Debug.Log($"Объект {name} получит повреждение в количестве: {damageTaken}, текущее здоровье: {health}/{healthMax}");

        health -= (int)damageTaken;
        health = Mathf.Clamp(health, 0, healthMax);

        if (health == 0)
        {
            Death();
        }

    }

    void Death()
    {
        GameObject death_object = Instantiate(gameObjectCreateAfterDeath);
        death_object.transform.position = transform.position;
        death_object.transform.parent = this.transform.parent;
        Debug.Log($"{name} погиб");
        Debug.Log($"{this.transform.position}, {this.transform.localPosition}, {death_object.transform.position}, {death_object.transform.localPosition}");
        PlayHitAnimation(Vector3.zero);
        Destroy(this.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController damageable = collision.transform.GetComponent<PlayerController>();
        //PlayerController test = collision.GetComponent<PlayerController>();

        cur_time_before_damage -= Time.deltaTime;
        if (cur_time_before_damage <= 0 && damageable != null)
        {
            damageable.Damage(damage);
            cur_time_before_damage = timeSecondsBeforeDamageAgain;

            PlayHitAnimation(moveVector);
        }
    }

    /// <summary>
    /// Анимация удара по чему-либо
    /// </summary>
    void PlayHitAnimation(Vector3 vectorToTarget)
    {
        GameObject effect = Instantiate(hitEffect);
        effect.transform.position = transform.position + vectorToTarget * 0.65f;
    }
}

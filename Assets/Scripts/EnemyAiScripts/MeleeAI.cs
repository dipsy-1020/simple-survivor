using UnityEngine;

public class MeleeAI : MonoBehaviour
{
    [Header("���ʳ]�w")]
    public float speed = 3f;
    public float stopDistance = 0.5f; // �s�W�G�P���a�������Z���]�P�w����Ĳ�^

    [Header("�ˮ`�]�w")]
    public int damage = 10;
    public float damageCooldown = 1f;
    private float lastDamageTime;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        // ���¦V���a
        if (direction.x != 0) sr.flipX = direction.x < 0;

        // �u���b�W�L stopDistance �ɤ~���ʡA�_�h�O���R��
        if (distance > stopDistance)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
        else
        {
            // �i�J��Ĳ�d��G�j��m�t�סA����D�ʷư�
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }
}
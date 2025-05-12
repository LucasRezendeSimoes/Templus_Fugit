using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBall : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;

    private Rigidbody2D rb2d;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Será chamado logo após Instantiate, definindo a direção
    public void Launch(Vector2 direction)
    {
        // 1) Rotate o projétil de modo que o seu eixo local +X (right) aponte para "direction"
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 2) Define a velocidade
        rb2d.velocity = direction * speed;

        // 3) Auto‐destrói em 1 segundo caso não atinja nada
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se bater num Boss:
        if (other.CompareTag("Boss"))
        {
            var boss = other.GetComponent<BossController>();
            if (boss != null)
                boss.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // Se bater num inimigo comum:
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }
        
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBall : MonoBehaviour
{
    [Header("Movimento & Dano")]
    public float speed = 10f;
    public int   damage;    // definido pelo PlayerController

    [Header("Áudio")]
    [Tooltip("Som ao lançar a bola de fogo")]
    public AudioClip launchClip;
    [Tooltip("Som ao impactar algo")]
    public AudioClip hitClip;

    private Rigidbody2D rb2d;
    private AudioSource _audioSource;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        // Cria AudioSource para este projétil
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
    }

    /// <summary>
    /// Chame logo após Instantiate para definir a direção.
    /// </summary>
    public void Launch(Vector2 direction)
    {
        // Orienta o projétil
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Dá velocidade
        rb2d.velocity = direction * speed;

        // Toca som de lançamento
        if (launchClip != null)
            _audioSource.PlayOneShot(launchClip);

        // Auto‐destrói em 1s (caso não bata em nada)
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool didHit = false;

        // Tenta causar dano em boss
        if (other.CompareTag("Boss"))
        {
            var boss = other.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                didHit = true;
            }
        }
        // Tenta causar dano em inimigo comum
        else if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                didHit = true;
            }
        }
        // Se bateu no chão ou em qualquer outra coisa
        else if (other.CompareTag("Ground"))
        {
            didHit = true;
        }

        if (didHit)
        {
            // toca som de impacto
            if (hitClip != null)
                _audioSource.PlayOneShot(hitClip);

            // garante que o som toque antes de destruir
            Destroy(gameObject, hitClip != null ? hitClip.length : 0f);
        }
    }
}

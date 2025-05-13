using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBall : MonoBehaviour
{
    [Header("Movimento & Dano")]
    public float speed = 10f;
    public int   damage;

    [Header("Áudio")]
    [Tooltip("Som ao lançar a bola de fogo")]
    public AudioClip launchClip;
    [Tooltip("Som ao impactar algo")]
    public AudioClip hitClip;

    private Rigidbody2D rb2d;
    private AudioSource _audioSource;
    private Collider2D  _collider;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        // Ajusta dano caso não tenha sido definido externamente
        if (damage <= 0 && GameManager.Instance != null)
            damage = GameManager.Instance.GetFlameBallDamage();

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
        // Toca no chão ou em obstáculo genérico
        else if (other.CompareTag("Ground"))
        {
            didHit = true;
        }

        if (!didHit)
            return;

        // Para o projétil e desativa colisão/visual
        rb2d.velocity = Vector2.zero;
        if (_collider != null)
            _collider.enabled = false;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        // Toca som de impacto e destrói após o áudio
        if (hitClip != null)
        {
            _audioSource.PlayOneShot(hitClip);
            Destroy(gameObject, hitClip.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
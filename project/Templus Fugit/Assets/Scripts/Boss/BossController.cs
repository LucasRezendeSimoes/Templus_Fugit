using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    [Header("Referências")]
    public Transform target;               // o jogador
    private NavMeshAgent agent;
    private Animator       animator;
    private Rigidbody2D    rb2d;

    [Header("Animações")]
    public RuntimeAnimatorController Idle;
    public RuntimeAnimatorController Run;
    public RuntimeAnimatorController Attack;
    public RuntimeAnimatorController TakeHit;
    public RuntimeAnimatorController Death;

    [Header("Parâmetros de Combate")]
    public int   maxHealth       = 20;
    public float detectionRange = 8f;
    public float attackRange    = 1.2f;
    public float attackCooldown = 1.5f;
    public float hitCooldown    = 0.5f;

    private int   _currentHealth;
    private bool  _canAttack      = true;
    private bool  _canBeHit       = true;
    private float _lastAttackTime;

    [Header("Health Bar")]
    [Tooltip("Prefab do HealthBar (com componente HealthBar)")]
    public HealthBar healthBarPrefab;  
    [Tooltip("Altura da barra acima da cabeça")]
    public float     healthBarHeight = 1.2f;
    private HealthBar _healthBarInstance;

    void Start()
    {
        // Setup combate
        _currentHealth = maxHealth;

        agent    = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;

        animator = GetComponent<Animator>();
        rb2d     = GetComponent<Rigidbody2D>();

        // Instancia a barra de vida no Canvas
        if (healthBarPrefab != null)
        {
            // procura um GameObject vazio chamado "HealthBars" sob o Canvas
            var container = GameObject.Find("HealthBars")?.transform;
            if (container == null)
                Debug.LogError("Não achei o container 'HealthBars' no Canvas!");

            else
            {
                _healthBarInstance = Instantiate(
                    healthBarPrefab,
                    container
                );
                _healthBarInstance.Initialize(transform, Vector3.up * healthBarHeight);
                _healthBarInstance.SetHealthPercent(1f);
            }
        }

        // Começa desativando a hitbox de ataque
        ToggleHitBox(false);
    }

    void Update()
    {
        // pausa se o tempo estiver stopped
        if (GameManager.Instance.IsTimeStopped) return;

        // se o player estiver invisível, fica só em Idle
        if (GameManager.Instance.IsInvisible)
        {
            animator.runtimeAnimatorController = Idle;
            agent.isStopped = true;
            ToggleHitBox(false);
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            animator.runtimeAnimatorController = Idle;
            agent.isStopped = true;
            ToggleHitBox(false);
        }
        else if (dist > attackRange)
        {
            animator.runtimeAnimatorController = Run;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            FlipDirection();
            ToggleHitBox(false);
        }
        else
        {
            agent.isStopped = true;
            FlipDirection();

            if (Time.time - _lastAttackTime >= attackCooldown && _canAttack)
            {
                StartCoroutine(HandleAttack());
                _lastAttackTime = Time.time;
            }
        }
    }

    private IEnumerator HandleAttack()
    {
        _canAttack = false;
        animator.runtimeAnimatorController = Attack;

        // meio da animação → ativa hitbox
        float half = animator.GetCurrentAnimatorStateInfo(0).length * 0.5f;
        yield return new WaitForSeconds(half);

        var hb = transform.Find("BossHitBox");
        if (hb != null) hb.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        if (hb != null) hb.gameObject.SetActive(false);

        animator.runtimeAnimatorController = Idle;
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    /// <summary>
    /// Chame este método (por exemplo, da FlameBall) para aplicar dano.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        if (!_canBeHit) return;

        _currentHealth -= dmg;
        _canBeHit = false;

        // atualiza barra de vida
        if (_healthBarInstance != null)
            _healthBarInstance.SetHealthPercent(_currentHealth / (float)maxHealth);

        // feedback visual
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0)
            Die();
        else
            StartCoroutine(HitCooldown());
    }

    private IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        _canBeHit = true;
    }

    private IEnumerator FlashRed()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = Color.white;
        }
    }

    private void Die()
    {
        // destrói barra
        if (_healthBarInstance != null)
            Destroy(_healthBarInstance.gameObject);

        animator.runtimeAnimatorController = Death;
        agent.isStopped = true;
        Destroy(gameObject, 1f);
    }

    private void FlipDirection()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // horizontal
            transform.eulerAngles = dir.x > 0
                ? new Vector3(0, 0, 0)
                : new Vector3(0, 180, 0);
        }
        else
        {
            // vertical
            transform.eulerAngles = dir.y > 0
                ? new Vector3(0, 90, 0)
                : new Vector3(0, 270, 0);
        }
    }

    private void ToggleHitBox(bool state)
    {
        var hb = transform.Find("BossHitBox");
        if (hb != null) hb.gameObject.SetActive(state);
    }
}
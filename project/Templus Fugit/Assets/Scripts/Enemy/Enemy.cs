using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform target;                // arraste ali o Player
    public float detectionRange = 8f;
    public float attackRange    = 1.2f;
    public float attackCooldown = 1f;
    public int   maxHealth      = 30;
    public float hitCooldown    = 0.5f;
    public float flashTime      = 0.1f;

    [Header("Animators")]
    public RuntimeAnimatorController idleAnim;
    public RuntimeAnimatorController runAnim;
    public RuntimeAnimatorController attackAnim;
    public RuntimeAnimatorController hitAnim;
    public RuntimeAnimatorController deathAnim;

    [Header("Health Bar")]
    [SerializeField] private HealthBar _healthBarPrefab;      // assign no Inspector
    [SerializeField] private float     _healthBarHeight = 1.2f;
    private HealthBar  _healthBarInstance;

    private NavMeshAgent agent;
    private Animator      animator;
    private Rigidbody2D   rb;
    private int           currentHealth;
    private bool          canAttack = true;
    private bool          canBeHit   = true;
    private float         lastAttack;

    void Start()
    {
        // (se já tinhas currentHealth = maxHealth, deixa aqui)
        currentHealth = maxHealth;

        // instancia a barra de vida
        if (_healthBarPrefab != null)
        {
            // procurar o container no Canvas
            var container = GameObject.Find("HealthBar")?.transform;
            if (container != null)
            {
                _healthBarInstance = Instantiate(
                    _healthBarPrefab,
                    container
                );
                _healthBarInstance.Initialize(transform, Vector3.up * _healthBarHeight);
                _healthBarInstance.SetHealthPercent(1f);
            }
        }
        agent     = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        animator  = GetComponent<Animator>();
        rb        = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        // não processa enquanto tempo parado ou invisível
        if (GameManager.Instance.IsTimeStopped || GameManager.Instance.IsInvisible)
        {
        agent.isStopped = true;
        animator.runtimeAnimatorController = idleAnim;
        return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
        // distante → idle
        agent.isStopped = true;
        animator.runtimeAnimatorController = idleAnim;
        ToggleHitBox(false);
        }
        else if (dist > attackRange)
        {
        // persegue
        agent.isStopped = false;
        agent.SetDestination(target.position);
        animator.runtimeAnimatorController = runAnim;
        FlipTowards(target.position);
        ToggleHitBox(false);
        }
        else
        {
        // ataque
        agent.isStopped = true;
        FlipTowards(target.position);

        if (Time.time - lastAttack >= attackCooldown && canAttack)
            StartCoroutine(DoAttack());
        }
    }

    private IEnumerator DoAttack()
    {
        canAttack = false;
        lastAttack = Time.time;

        animator.runtimeAnimatorController = attackAnim;
        // espera animação (você pode usar length ou _AnimationEvents_)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // ativa hitbox
        var hb = transform.Find("EnemyHitBox").gameObject;
        hb.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        hb.SetActive(false);

        animator.runtimeAnimatorController = idleAnim;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int dmg)
    {
        if (!canBeHit) return;

        currentHealth -= dmg;
        // atualiza UI:
        if (_healthBarInstance != null)
            _healthBarInstance.SetHealthPercent(currentHealth / (float)maxHealth);

        canBeHit = false;
        StartCoroutine(FlashRed());

        if (!canBeHit) return;
        currentHealth -= dmg;
        canBeHit = false;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0) Die();
        else                       StartCoroutine(ResetHitCooldown());
    }

    private IEnumerator ResetHitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    private IEnumerator FlashRed()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sr.color = Color.white;
    }

    private void Die()
    {
        // opcional: destruir barra de vida junto
        if (_healthBarInstance != null)
            Destroy(_healthBarInstance.gameObject);

        animator.runtimeAnimatorController = deathAnim;
        agent.isStopped = true;
        Destroy(gameObject, 1f);
    }

    private void FlipTowards(Vector3 pos)
    {
        var dir = pos - transform.position;
        if (dir.x > 0) transform.eulerAngles = Vector3.zero;
        else           transform.eulerAngles = new Vector3(0,180,0);
    }

    private void ToggleHitBox(bool v)
    {
        var hb = transform.Find("EnemyHitBox")?.gameObject;
        if (hb!=null) hb.SetActive(v);
    }
    }

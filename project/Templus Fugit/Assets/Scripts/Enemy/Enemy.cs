using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public float     detectionRange = 8f;
    public float     attackRange    = 1.2f;
    public float     attackCooldown = 1f;
    public int       maxHealth      = 30;
    public float     hitCooldown    = 0.5f;
    public float     flashTime      = 0.1f;

    [Header("Animators")]
    public RuntimeAnimatorController idleAnim;
    public RuntimeAnimatorController runAnim;
    public RuntimeAnimatorController attackAnim;
    public RuntimeAnimatorController hitAnim;
    public RuntimeAnimatorController deathAnim;

    [Header("Health Bar")]
    [SerializeField] private HealthBar _healthBarPrefab;      // assign no Inspector
    [SerializeField] private float     _healthBarHeight = 1.2f;
    private HealthBar _healthBarInstance;

    private NavMeshAgent agent;
    private Animator     animator;
    private Rigidbody2D  rb;

    private int   currentHealth;
    private bool  canAttack = true;
    private bool  canBeHit  = true;
    private float lastAttack;

    void Start()
    {
        currentHealth = maxHealth;

        // instancia a barra
        if (_healthBarPrefab != null)
        {
            var container = GameObject.Find("HealthBars")?.transform; // ou "HealthBar", conforme vc nomear
            if (container != null)
            {
                _healthBarInstance = Instantiate(_healthBarPrefab, container);
                _healthBarInstance.Initialize(transform, Vector3.up * _healthBarHeight);
                _healthBarInstance.SetHealthPercent(1f);
            }
        }

        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb       = GetComponent<Rigidbody2D>();

        agent.updateRotation = false;
        agent.updateUpAxis   = false;
    }

    void Update()
    {
        if (GameManager.Instance.IsTimeStopped || GameManager.Instance.IsInvisible)
        {
            agent.isStopped                  = true;
            animator.runtimeAnimatorController = idleAnim;
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            // idle
            agent.isStopped                  = true;
            animator.runtimeAnimatorController = idleAnim;
            ToggleHitBox(false);
        }
        else if (dist > attackRange)
        {
            // persegue
            agent.isStopped                  = false;
            agent.SetDestination(target.position);
            animator.runtimeAnimatorController = runAnim;
            FlipTowards(target.position);
            ToggleHitBox(false);
        }
        else
        {
            // ataca
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
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        var hb = transform.Find("EnemyHitBox")?.gameObject;
        if (hb != null)
        {
            hb.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            hb.SetActive(false);
        }

        animator.runtimeAnimatorController = idleAnim;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int dmg)
    {
        if (!canBeHit) return;

        // 1) Subtrai vida e atualiza UI
        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
        _healthBarInstance?.SetHealthPercent(currentHealth / (float)maxHealth);

        // 2) Se morreu, mata e sai
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 3) flash + cooldown de hit
        canBeHit = false;
        StartCoroutine(FlashRed());
        StartCoroutine(ResetHitCooldown());
    }

    private IEnumerator ResetHitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    private IEnumerator FlashRed()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(flashTime);
            sr.color = Color.white;
        }
    }

    private void Die()
    {
        if (_healthBarInstance != null)
            Destroy(_healthBarInstance.gameObject);
        animator.runtimeAnimatorController = deathAnim;
        agent.isStopped = true;
        Destroy(gameObject, 1f);
    }

    private void FlipTowards(Vector3 pos)
    {
        var dir = pos - transform.position;
        transform.eulerAngles = dir.x > 0 ? Vector3.zero : new Vector3(0,180,0);
    }

    private void ToggleHitBox(bool v)
    {
        var hb = transform.Find("EnemyHitBox")?.gameObject;
        if (hb!=null) hb.SetActive(v);
    }
}
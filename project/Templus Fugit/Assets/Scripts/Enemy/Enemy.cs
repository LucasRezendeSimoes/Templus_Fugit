using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    enum Facing { Left, Right, Up, Down }
    enum State  { Idle, Walk, Attack, Hit, Death }

    [Header("Referências")]
    public Transform target;

    private NavMeshAgent agent;
    private Animator     animator;
    private Rigidbody2D  rb2d;

    [Header("Animadores por Estado e Direção")]
    public RuntimeAnimatorController IdleLeftController,  IdleRightController,  IdleTopController,  IdleBotController;
    public RuntimeAnimatorController WalkLeftController,  WalkRightController,  WalkTopController,  WalkBotController;
    public RuntimeAnimatorController AttackLeftController,AttackRightController,AttackTopController,AttackBotController;
    public RuntimeAnimatorController HitLeftController,   HitRightController,   HitTopController,   HitBotController;
    public RuntimeAnimatorController DeathLeftController,DeathRightController,DeathTopController,DeathBotController;

    [Header("Parâmetros de Combate")]
    public int   vida            = 50;
    public float detectionRange  = 10f;
    public float attackRange     = 1.5f;
    public float attackCooldown  = 1f;
    public float hitCooldown     = 0.5f;
    public float damageFlashTime = 0.2f;

    private Facing currentFacing = Facing.Left;
    private int    currentHealth;
    private bool   canAttack     = true;
    private bool   canBeHit      = true;
    private float  lastAttackTime;

    [Header("Barra de Vida")]
    [Tooltip("Arraste aqui o prefab da HealthBar")]
    public HealthBar healthBarPrefab;
    [Tooltip("Offset da barra acima do inimigo")]
    public Vector3   healthBarOffset = new Vector3(0, 1, 0);

    // INSTÂNCIA criada em cena
    private HealthBar healthBarInstance;

    void Start()
    {
        currentHealth = vida;

        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb2d     = GetComponent<Rigidbody2D>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;

        // 1) Instancia a barra de vida na cena
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(
                healthBarPrefab,
                transform.position + healthBarOffset,
                Quaternion.identity,
                // opcional: se tiver um GameObject pai para as barras
                GameObject.Find("HealthBars")?.transform
            );

            // 2) inicializa e seta como cheia
            healthBarInstance.Initialize(transform, healthBarOffset);
            healthBarInstance.SetHealthPercent(1f);
        }
    }

    void Update()
    {
        // 3) Atualiza a instância, não o prefab
        if (healthBarInstance != null)
            healthBarInstance.SetHealthPercent(currentHealth / (float)vida);

        // lógica de stun/invisibilidade
        if (GameManager.Instance.IsTimeStopped || GameManager.Instance.IsInvisible)
        {
            PlayAnimation(State.Idle);
            agent.isStopped = true;
            return;
        }

        // se morreu
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        currentFacing = CalculateFacing();

        if (dist > detectionRange)
        {
            agent.isStopped = true;
            PlayAnimation(State.Idle);
        }
        else if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
            PlayAnimation(State.Walk);
        }
        else
        {
            agent.isStopped = true;
            if (canAttack && Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(DoAttack());
                lastAttackTime = Time.time;
            }
        }
    }

    Facing CalculateFacing()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? Facing.Right : Facing.Left;
        else
            return dir.y > 0 ? Facing.Up : Facing.Down;
    }

    void PlayAnimation(State state)
    {
        RuntimeAnimatorController ctrl = null;
        switch (state)
        {
            case State.Idle:
                if (currentFacing == Facing.Left)  ctrl = IdleLeftController;
                if (currentFacing == Facing.Right) ctrl = IdleRightController;
                if (currentFacing == Facing.Up)    ctrl = IdleTopController;
                if (currentFacing == Facing.Down)  ctrl = IdleBotController;
                break;
            case State.Walk:
                if (currentFacing == Facing.Left)  ctrl = WalkLeftController;
                if (currentFacing == Facing.Right) ctrl = WalkRightController;
                if (currentFacing == Facing.Up)    ctrl = WalkTopController;
                if (currentFacing == Facing.Down)  ctrl = WalkBotController;
                break;
            case State.Attack:
                if (currentFacing == Facing.Left)  ctrl = AttackLeftController;
                if (currentFacing == Facing.Right) ctrl = AttackRightController;
                if (currentFacing == Facing.Up)    ctrl = AttackTopController;
                if (currentFacing == Facing.Down)  ctrl = AttackBotController;
                break;
            case State.Hit:
                if (currentFacing == Facing.Left)  ctrl = HitLeftController;
                if (currentFacing == Facing.Right) ctrl = HitRightController;
                if (currentFacing == Facing.Up)    ctrl = HitTopController;
                if (currentFacing == Facing.Down)  ctrl = HitBotController;
                break;
            case State.Death:
                if (currentFacing == Facing.Left)  ctrl = DeathLeftController;
                if (currentFacing == Facing.Right) ctrl = DeathRightController;
                if (currentFacing == Facing.Up)    ctrl = DeathTopController;
                if (currentFacing == Facing.Down)  ctrl = DeathBotController;
                break;
        }
        if (ctrl != null && animator.runtimeAnimatorController != ctrl)
            animator.runtimeAnimatorController = ctrl;
    }

    IEnumerator DoAttack()
    {
        canAttack = false;

        PlayAnimation(State.Attack);
        var clipLen = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLen);

        var hb = transform.Find("EnemyHitBox")?.gameObject;
        if (hb != null)
        {
            hb.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            hb.SetActive(false);
        }

        PlayAnimation(State.Idle);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int dmg)
    {
        if (!canBeHit) return;

        currentHealth -= dmg;
        canBeHit = false;

        if (currentHealth <= 0)
        {
            PlayAnimation(State.Death);
            Die();
        }
        else
        {
            StartCoroutine(FlashRed());
            PlayAnimation(State.Hit);
            StartCoroutine(ResetHitCooldown());
        }
    }

    IEnumerator ResetHitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    IEnumerator FlashRed()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(damageFlashTime);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        agent.isStopped = true;
        Destroy(gameObject, 0.4f);
    }
}

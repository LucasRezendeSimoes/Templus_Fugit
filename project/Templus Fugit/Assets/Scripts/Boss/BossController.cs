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
    public int   health          = 20;
    public float detectionRange = 8f;
    public float attackRange    = 1.2f;
    public float attackCooldown = 1.5f;
    public float hitCooldown    = 0.5f;
    private bool  canAttack     = true;
    private bool  canBeHit      = true;
    private float lastAttackTime;

    void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;

        animator = GetComponent<Animator>();
        rb2d     = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // se o tempo está parado, nada acontece
        if (GameManager.Instance.IsTimeStopped) return;

        // se o player está invisível, fica em Idle
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
            // fora de alcance de detecção → Idle
            animator.runtimeAnimatorController = Idle;
            agent.isStopped = true;
            ToggleHitBox(false);
        }
        else if (dist > attackRange)
        {
            // persegue
            animator.runtimeAnimatorController = Run;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            FlipDirection();
            ToggleHitBox(false);
        }
        else
        {
            // está em alcance de ataque
            agent.isStopped = true;
            FlipDirection();

            if (Time.time - lastAttackTime >= attackCooldown && canAttack)
            {
                StartCoroutine(HandleAttack());
                lastAttackTime = Time.time;
            }
        }
    }

    private IEnumerator HandleAttack()
    {
        canAttack = false;
        animator.runtimeAnimatorController = Attack;

        // ativa hitbox na metade da animação
        float attackAnimLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(attackAnimLength * 0.5f);

        var hb = transform.Find("EnemyHitBox");
        if (hb != null) hb.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        if (hb != null) hb.gameObject.SetActive(false);

        // volta a Idle e espera cooldown
        animator.runtimeAnimatorController = Idle;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int dmg)
    {
        if (!canBeHit) return;

        health -= dmg;
        canBeHit = false;
        StartCoroutine(FlashRed());

        if (health <= 0) Die();
        else             StartCoroutine(HitCooldown());
    }

    private IEnumerator HitCooldown()
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
            yield return new WaitForSeconds(0.2f);
            sr.color = Color.white;
        }
    }

    private void Die()
    {
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
        var hb = transform.Find("EnemyHitBox");
        if (hb != null) hb.gameObject.SetActive(state);
    }
}
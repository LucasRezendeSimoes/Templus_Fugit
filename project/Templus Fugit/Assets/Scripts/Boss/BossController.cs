using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody2D rb2d;

    [Header("Animadores")]
    public RuntimeAnimatorController Attack;
    public RuntimeAnimatorController Death;
    public RuntimeAnimatorController Idle;
    public RuntimeAnimatorController Run;
    public RuntimeAnimatorController Take_hit;

    [Header("Parâmetros de Combate")]
    public int vida = 100;
    private bool canBeHit = true; // Para evitar que o inimigo tome dano enquanto está em cooldown
    public float attackRange = 1.5f;
    public float DamageFlashTime = 0.2f; // Tempo que o inimigo pisca em vermelho ao receber dano
    public float detectionRange = 10.0f;
    public float hitCooldown = 0.5f;
    public float attackCooldown = 1.0f; // Tempo de cooldown entre os ataques
    private bool canAttack = true; // Para evitar que o inimigo ataque enquanto está em cooldown
    private float lastAttackTime;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // se o relógio está parado, nem procesa nada
        if (GameManager.Instance.IsTimeStopped)
            return;
            
        // Se o jogador estiver invisível, o boss fica em idle e não persegue nem ataca
        if (GameManager.Instance != null && GameManager.Instance.IsInvisible)
        {
            animator.runtimeAnimatorController = Idle;
            agent.isStopped = true;
            ToggleHitBox(false);
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > detectionRange)
        {
            // Idle
            animator.runtimeAnimatorController = Idle;
            agent.isStopped = true;
            ToggleHitBox(false);
        }
        else if (distanceToTarget > attackRange)
        {
            // Corre em direção ao jogador
            animator.runtimeAnimatorController = Run;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            FlipDirection();
            ToggleHitBox(false);
        }
        else
        {
            // Está no alcance de ataque
            agent.isStopped = true;
            FlipDirection();

            if (Time.time - lastAttackTime >= attackCooldown && canAttack)
            {
                StartCoroutine(HandleAttack());
                lastAttackTime = Time.time;
            }
        }
    }

    private void FlipDirection()
    {
        Vector3 direction = target.position - transform.position;
        if (direction.x > 0) // Direita
            transform.eulerAngles = new Vector3(0, 0, 0);
        else if (direction.x < 0) // Esquerda
            transform.eulerAngles = new Vector3(0, 180, 0);
        else if (direction.y > 0) // Cima
            transform.eulerAngles = new Vector3(0, 90, 0);
        else if (direction.y < 0) // Baixo
            transform.eulerAngles = new Vector3(0, 270, 0);
    }

    public void TakeDamage(int damage)
    {
        if (!canBeHit) return;

        vida -= damage;
        canBeHit = false;

        StartCoroutine(FlashRed());

        if (vida <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitCooldownCoroutine());
        }
    }

    private IEnumerator HitCooldownCoroutine()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(DamageFlashTime);
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator HandleAttack()
    {
        // Configura o estado de ataque
        canAttack = false;
        animator.runtimeAnimatorController = Attack;

        // Referência à hitbox de ataque
        Transform hitBoxTransform = transform.Find("BossHitBox");
        if (hitBoxTransform == null)
        {
            // Debug.LogError("BossHit não foi encontrado como filho do BossController.");
            yield break; // Sai da função se a hitbox não for encontrada
        }

        GameObject hitBox = hitBoxTransform.gameObject;

        // Aguarda o tempo necessário para a animação de ataque terminar
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Ativa a hitbox para causar dano
        hitBox.SetActive(true);

        // Aguarda um curto período para garantir que o dano seja aplicado
        yield return new WaitForSeconds(0.1f);

        // Desativa a hitbox após o ataque
        hitBox.SetActive(false);

        animator.runtimeAnimatorController = Idle; // Retorna ao estado Idle após o ataque

        // Aguarda o cooldown do ataque antes de permitir outro ataque
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private void Die()
    {
        animator.runtimeAnimatorController = Death;
        agent.isStopped = true;
        Destroy(gameObject, 1.0f); // Destroi o inimigo após a animação de morte
    }

    private void ToggleHitBox(bool state)
    {
        GameObject hitBox = transform.Find("BossHitBox").gameObject;
        if (hitBox != null)
        {
            hitBox.SetActive(state);
        }
    }
}

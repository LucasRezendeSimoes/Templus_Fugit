using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class BossController : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody2D rb2d;

    [Header("Animadores")]
    public RuntimeAnimatorController AttackController;
    public RuntimeAnimatorController DeathController;
    public RuntimeAnimatorController IdleController;
    public RuntimeAnimatorController RunController;
    public RuntimeAnimatorController TakeHitController;

    [Header("Parâmetros de Combate")]
    public int vida = 100;
    private bool canBeHit = true;
    public float attackRange = 1.5f;
    public float DamageFlashTime = 0.2f;
    public float detectionRange = 10.0f;
    public float hitCooldown = 0.5f;
    public float attackCooldown = 1.0f;
    private bool canAttack = true;
    private float lastAttackTime;

    // Referência para a barra de saúde
    public HealthBar healthBar;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    
        // Inicializa a HealthBar
        healthBar.Initialize(transform, new Vector3(0, 1, 0)); // Define a posição da barra acima do Boss
    }

    void Update()
    {
        // Atualiza a barra de saúde
        if (vida > 0)
        {
            healthBar.SetHealthPercent((float)vida / 100);
        }
        else
        {
            healthBar.gameObject.SetActive(false); // Desativa a barra de saúde se o Boss estiver morto
        }

        // Se o relógio está parado, nem processa nada
        if (GameManager.Instance.IsTimeStopped)
            return;

        // Se o jogador estiver invisível, o boss fica em idle e não persegue nem ataca
        if (GameManager.Instance != null && GameManager.Instance.IsInvisible)
        {
            animator.runtimeAnimatorController = IdleController;
            agent.isStopped = true;
            ToggleHitBox(false);
            return;
        }

        // Se o Boss estiver morto, não faz mais nada
        if (vida <= 0)
        {
            Die();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > detectionRange)
        {
            // Idle
            animator.runtimeAnimatorController = IdleController;
            agent.isStopped = true;
            ToggleHitBox(false);
        }
        else if (distanceToTarget > attackRange)
        {
            // Corre em direção ao jogador
            animator.runtimeAnimatorController = RunController;
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
        if (direction.x > 0) 
            transform.eulerAngles = new Vector3(0, 0, 0);
        else if (direction.x < 0) 
            transform.eulerAngles = new Vector3(0, 180, 0);
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
        canAttack = false;
        animator.runtimeAnimatorController = AttackController;  // Usando o controller de ataque

        // Referência à hitbox de ataque
        Transform hitBoxTransform = transform.Find("BossHitBox");
        if (hitBoxTransform == null)
        {
            yield break; // Sai da função se a hitbox não for encontrada
        }

        GameObject hitBox = hitBoxTransform.gameObject;

        // Ativa a hitbox para causar dano
        hitBox.SetActive(true);

        // Aguarda o tempo necessário para a animação de ataque terminar
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Desativa a hitbox após o ataque
        hitBox.SetActive(false);

        animator.runtimeAnimatorController = IdleController;  // Retorna ao controller de Idle após o ataque

        // Aguarda o cooldown do ataque antes de permitir outro ataque
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private void Die()
    {
        animator.runtimeAnimatorController = DeathController;  // Usando o controller de morte
        agent.isStopped = true;
        Destroy(gameObject, 0.5f); // Destroi o inimigo após a animação de morte
        // muda para cena YouWin
        SceneManager.LoadScene("YouWin");

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

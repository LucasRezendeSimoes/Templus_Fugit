using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform target; // Alvo para o inimigo seguir
    private NavMeshAgent agent;
    private Animator animator; // Componente Animator para controlar as animações
    private Rigidbody2D rb2d; // Define o corpo rígido 2D que representa o player

    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    // private static readonly int Speed = Animator.StringToHash("Speed");
    // private static readonly int Idle = Animator.StringToHash("Idle");
    // private static readonly int Walk = Animator.StringToHash("Walk");
    // private static readonly int Run = Animator.StringToHash("Run");
    // private static readonly int Attack = Animator.StringToHash("Attack");
    // private static readonly int Death = Animator.StringToHash("Death");
    
    // [Header("Recompensa em Moedas")]
    // public int coinReward = 5;

    private bool _initialized;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        animator = GetComponent<Animator>();
        if (animator == null) 
        {
            return;
        }

        _initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        // se o relógio está parado, nem procesa nada
        if (GameManager.Instance.IsTimeStopped)
            return;
        if (!_initialized || target == null)
            return;

        // Move o inimigo em direção ao alvo
        agent.SetDestination(target.position);

        // Calcula a direção do movimento
        Vector3 direction = agent.velocity.normalized;

        // Atualiza os parâmetros do Animator
        animator.SetFloat(Horizontal, direction.x);
        animator.SetFloat(Vertical, direction.y);

        // Define a velocidade para determinar se o inimigo está parado ou em movimento
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}

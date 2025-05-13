using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Teclas de Movimento e Interação")]
    public KeyCode moveUpKey = KeyCode.W;
    public KeyCode moveDownKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode fireKey = KeyCode.Space;

    [Header("Movimentação e Interação")]
    public float moveSpeed = 5f;
    private Vector2 _velocity;
    public float interactionRange = 2f;
    public LayerMask interactableLayer;

    [Header("Limites do Cenário")]
    public float boundXEsquerda = -7.458932f;
    public float boundXDireita  =  7.458932f;
    public float boundYBaixo     = -2.997569f;
    public float boundYCima      = 35.39562f;

    [Header("Flame Ball")]
    [SerializeField] private Transform firePoint;

    [Header("Cooldown de Tiro")]
    [Tooltip("Tempo mínimo, em segundos, entre dois disparos de Flame Ball")]
    public float fireCooldown = 0.5f;
    private float _lastFireTime = 0f;

    private Rigidbody2D rb2d;
    private Animator   animator;
    private bool       canMove     = true;
    private Vector2    _lastFacing = Vector2.right;

    [Header("Animações")]
    public RuntimeAnimatorController andarCima;
    public RuntimeAnimatorController andarBaixo;
    public RuntimeAnimatorController andarEsquerda;
    public RuntimeAnimatorController andarDireita;
    public RuntimeAnimatorController paradoCostas;
    public RuntimeAnimatorController paradoFrente;
    public RuntimeAnimatorController paradoEsquerda;
    public RuntimeAnimatorController paradoDireita;

    [Header("Parâmetros de Combate")]
    private bool canBeHit = true;           // cooldown para receber dano
    public float hitCooldown = 2f;          // segundos de invencibilidade

    [Header("Áudio de Passos")]
    [Tooltip("Lista de clipes de passo; escolha aleatoriamente.")]
    public AudioClip[] footstepClips;
    [Tooltip("Intervalo em segundos entre cada passo enquanto se move.")]
    public float footstepInterval = 0.5f;

    private AudioSource _footstepSource;
    private float       _footstepTimer  = 0f;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // Cria e configura o AudioSource para passos
        _footstepSource = gameObject.AddComponent<AudioSource>();
        _footstepSource.playOnAwake = false;
        _footstepSource.loop       = false;
    }

    void Update()
    {
        if (!canMove)
        {
            _velocity = Vector2.zero;
            return;
        }

        HandleMovement();
        HandleFire();
        HandleInteraction();

        // usar slot 0…4 via teclas 1–5
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                GameManager.Instance.UseInventoryItem(i);
        }
    }

    void HandleMovement()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(moveUpKey))
        {
            dir.y += 1;
            animator.runtimeAnimatorController = andarCima;
        }
        else if (Input.GetKey(moveDownKey))
        {
            dir.y -= 1;
            animator.runtimeAnimatorController = andarBaixo;
        }
        else if (Input.GetKey(moveLeftKey))
        {
            dir.x -= 1;
            animator.runtimeAnimatorController = andarEsquerda;
        }
        else if (Input.GetKey(moveRightKey))
        {
            dir.x += 1;
            animator.runtimeAnimatorController = andarDireita;
        }
        else
        {
            // Parado: escolhe a pose final
            if (animator.runtimeAnimatorController == andarCima)
                animator.runtimeAnimatorController = paradoCostas;
            else if (animator.runtimeAnimatorController == andarBaixo)
                animator.runtimeAnimatorController = paradoFrente;
            else if (animator.runtimeAnimatorController == andarEsquerda)
                animator.runtimeAnimatorController = paradoEsquerda;
            else if (animator.runtimeAnimatorController == andarDireita)
                animator.runtimeAnimatorController = paradoDireita;
        }

        _velocity = dir.normalized * moveSpeed;

        // Guarda direção para projetil
        if (dir != Vector2.zero)
            _lastFacing = dir.normalized;

        // --- Áudio de Passos ---
        if (dir != Vector2.zero)
        {
            _footstepTimer += Time.deltaTime;
            if (_footstepTimer >= footstepInterval)
            {
                PlayFootstep();
                _footstepTimer = 0f;
            }
        }
        else
        {
            // Parou de andar: reseta timer para evitar passo instantâneo
            _footstepTimer = footstepInterval;
        }

        // Move personagem
        Vector2 newPos = rb2d.position + _velocity * Time.fixedDeltaTime;
        newPos.x = Mathf.Clamp(newPos.x, boundXEsquerda, boundXDireita);
        newPos.y = Mathf.Clamp(newPos.y, boundYBaixo, boundYCima);
        rb2d.MovePosition(newPos);
    }

    private void PlayFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0)
        {
            int idx = Random.Range(0, footstepClips.Length);
            _footstepSource.PlayOneShot(footstepClips[idx]);
        }
    }

    private void HandleFire()
    {
        // só dispara se o jogador já tiver Flame Power
        if (!GameManager.Instance.CanUseFlame || firePoint == null)
            return;

        // pega o prefab da Flame Ball guardado no GameManager
        var fbPrefab = GameManager.Instance.FlameBallPrefab;
        if (fbPrefab == null) return;

        if (Time.time - _lastFireTime < fireCooldown) return;

        if (Input.GetKeyDown(fireKey))
        {
            _lastFireTime = Time.time;
            // instancia usando o prefab e o dano atual
            var fb = Instantiate(fbPrefab, firePoint.position, Quaternion.identity)
                     .GetComponent<FlameBall>();
            fb.damage = GameManager.Instance.GetFlameBallDamage();
            fb.Launch(_lastFacing);
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Collider2D[] interactables =
                Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);
            foreach (var interactable in interactables)
            {
                var obj = interactable.GetComponent<IInteractable>();
                if (obj != null)
                {
                    obj.Interact();
                    return;
                }
            }
        }
    }

    public void SetCanMove(bool value) => canMove = value;

    public void TakeDamage(int amount)
    {
        if (!canBeHit) return;
        canBeHit = false;
        GameManager.Instance.LoseLife(amount);
        StartCoroutine(HitCooldownCoroutine());
    }

    private IEnumerator HitCooldownCoroutine()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void AddCoins(int amount = 1)
    {
        GameManager.Instance.AddCoins(amount);
        Debug.Log($"Player adicionou {amount} moedas. Total: {GameManager.Instance.coinCount}");
    }

    public bool SpendCoins(int amount)
    {
        if (GameManager.Instance.coinCount >= amount)
        {
            GameManager.Instance.AddCoins(-amount);
            Debug.Log($"Player gastou {amount} moedas. Restam: {GameManager.Instance.coinCount}");
            return true;
        }
        Debug.Log("Moedas insuficientes.");
        return false;
    }

    public int GetCoinCount() => GameManager.Instance.coinCount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            AddCoins(1);
            Destroy(other.gameObject);
        }
    }
}
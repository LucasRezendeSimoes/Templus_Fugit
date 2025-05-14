using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tipos de recompensa possíveis em um baú.
/// </summary>
public enum ChestReward
{
    Coins,
    BamiEmber,
    HealthPotion,
    CloakNyx,
    Hourglass,
    BrokenWatch
}

/// <summary>
/// Dispara um minigame de lockpick e distribui recompensas ao destravar.
/// Para Cena2 mantém comportamento fixo; para outras cenas, sorteio aleatório.
/// Destrói portas (tag "Door") quando todos inimigos (tag "Enemy") são eliminados.
/// </summary>
public class ChestLockpickTrigger : MonoBehaviour, IInteractable
{
    [Header("Identificador único do baú (para persistência)")]
    public string chestID;

    [Header("Minigame de Lockpick")]
    public GameObject lockpickUI;
    public LockpickDifficulty difficulty = LockpickDifficulty.Medium;
    private LockpickMinigame minigame;

    [Header("Recompensas Aleatórias (exceto Cena2)")]
    [Tooltip("Lista de recompensas possíveis neste baú")] 
    public ChestReward[] possibleRewards;

    [Header("Recompensa Fixa de Moedas")]
    public int coinReward = 5;

    [Header("Recompensa em Brasa de Bami (Flame Ball)")]
    [Tooltip("Prefab da Flame Ball que será concedido ao jogador")]
    public GameObject FlameBallPrefab;

    [Header("Áudio")]
    [Tooltip("Som que toca ao abrir o baú")]
    public AudioClip openChestClip;
    private AudioSource _audioSource;

    private bool opened = false;
    private bool doorsUnlocked = false;

    void Start()
    {
        if (lockpickUI != null)
            minigame = lockpickUI.GetComponent<LockpickMinigame>();

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (!string.IsNullOrEmpty(chestID) && GameManager.Instance.openedChests.Contains(chestID))
            opened = true;
    }

    void Update()
    {
        // Quando não há mais inimigos, destrói todas as portas
        if (!doorsUnlocked && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            foreach (var door in GameObject.FindGameObjectsWithTag("Door"))
                Destroy(door);
            doorsUnlocked = true;
        }
    }

    public void Interact()
    {
        TryOpen();
    }

    private void TryOpen()
    {
        if (opened || minigame == null)
            return;

        // Impede abertura enquanto inimigos existirem
        if (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            Debug.Log("Derrote todos os inimigos antes de abrir o baú.");
            return;
        }

        // Inicia minigame de lockpick
        lockpickUI.SetActive(true);
        minigame.StartMinigame(difficulty, UnlockChest);
    }

    private void UnlockChest()
    {
        if (opened) return;
        opened = true;

        // Salva id de baú aberto para persistência
        if (!string.IsNullOrEmpty(chestID))
            GameManager.Instance.openedChests.Add(chestID);

        string sceneName = SceneManager.GetActiveScene().name;
        var player = GameObject.FindGameObjectWithTag("Player");
        var pc     = player != null ? player.GetComponent<PlayerController>() : null;

        // Cena2: mantém comportamento fixo
        if (sceneName == "Cena2")
        {
            if (FlameBallPrefab != null)
            {
                GameManager.Instance.GrantFlamePower(FlameBallPrefab);
                bool added = GameManager.Instance.AddInventoryItem(ItemType.BamiEmber);
                if (!added)
                    Debug.LogWarning("Inventário cheio: não foi possível adicionar a Brasa de Bami.");
            }
        }
        else
        {
            // Sorteia recompensa entre as opções
            ChestReward reward = ChestReward.Coins;
            if (possibleRewards != null && possibleRewards.Length > 0)
                reward = possibleRewards[Random.Range(0, possibleRewards.Length)];

            switch (reward)
            {
                case ChestReward.Coins:
                    if (pc != null)
                        pc.AddCoins(coinReward);
                    break;
                case ChestReward.BamiEmber:
                    if (FlameBallPrefab != null)
                    {
                        GameManager.Instance.GrantFlamePower(FlameBallPrefab);
                        GameManager.Instance.AddInventoryItem(ItemType.BamiEmber);
                    }
                    break;
                case ChestReward.HealthPotion:
                    GameManager.Instance.AddInventoryItem(ItemType.HealthPotion);
                    break;
                case ChestReward.CloakNyx:
                    GameManager.Instance.AddInventoryItem(ItemType.CloakNyx);
                    break;
                case ChestReward.Hourglass:
                    GameManager.Instance.AddInventoryItem(ItemType.Hourglass);
                    break;
                case ChestReward.BrokenWatch:
                    GameManager.Instance.AddInventoryItem(ItemType.BrokenWatch);
                    break;
            }
        }

        // Toca som e fecha UI
        if (openChestClip != null)
            _audioSource.PlayOneShot(openChestClip);
        lockpickUI.SetActive(false);

        if (openChestClip != null)
            StartCoroutine(DeactivateAfterSound(openChestClip.length));
    }

    private IEnumerator DeactivateAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        // opcional: desativar o baú
        // gameObject.SetActive(false);
    }
}

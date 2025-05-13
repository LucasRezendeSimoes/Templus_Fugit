using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class ChestLockpickTrigger : MonoBehaviour, IInteractable
{
    [Header("Identificador único do baú (para persistência)")]
    public string chestID;

    [Header("Minigame de Lockpick")]
    public GameObject lockpickUI;
    public LockpickDifficulty difficulty = LockpickDifficulty.Medium;
    private LockpickMinigame minigame;

    [Header("Recompensa em Moedas")]
    public int coinReward = 5;

    [Header("Recompensa em Brasa de Bami (Flame Ball)")]
    [Tooltip("Prefab da Flame Ball que será concedido ao jogador")]
    public GameObject bamiEmberPrefab;

    [Header("Áudio")]
    [Tooltip("Som que toca ao abrir o baú")]
    public AudioClip openChestClip;
    private AudioSource _audioSource;

    private bool opened = false;

    void Start()
    {
        // Pega a referência ao minigame
        if (lockpickUI != null)
            minigame = lockpickUI.GetComponent<LockpickMinigame>();

        // Configura AudioSource para o baú
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        // Se já estiver aberto nesta run, desative o baú
        if (!string.IsNullOrEmpty(chestID) &&
            GameManager.Instance.openedChests.Contains(chestID))
        {
            opened = true;
            // gameObject.SetActive(false);
        }
    }

    // Chamado pelo PlayerController
    public void Interact()
    {
        TryOpen();
    }

    private void TryOpen()
    {
        if (opened || minigame == null) 
            return;

        // Abre a UI e inicia o minigame, passando a dificuldade e o callback
        lockpickUI.SetActive(true);
        minigame.StartMinigame(difficulty, UnlockChest);
    }

    // Callback executado quando o minigame devolve sucesso
    private void UnlockChest()
    {
        if (opened) return;
        opened = true;

        // Marca como aberto no GameManager
        if (!string.IsNullOrEmpty(chestID))
            GameManager.Instance.openedChests.Add(chestID);

        // Detecta cena atual
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Cena2" && bamiEmberPrefab != null)
        {
            // 1) Concede o poder de Flame Ball
            GameManager.Instance.GrantFlamePower(bamiEmberPrefab);

            // 2) Adiciona também a Brasa de Bami ao inventário
            bool added = GameManager.Instance.AddInventoryItem(ItemType.BamiEmber);
            if (!added)
                Debug.LogWarning("Inventário cheio: não foi possível adicionar a Brasa de Bami.");

            // Opcional: atualizar o ícone de poder ou de slot extra
        }
        else
        {
            // Lógica padrão: dá moedas
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var pc = player.GetComponent<PlayerController>();
                if (pc != null)
                    pc.AddCoins(coinReward);
            }
        }

        // Toca som de abertura
        if (openChestClip != null)
            _audioSource.PlayOneShot(openChestClip);

        // Fecha a UI de lockpick
        lockpickUI.SetActive(false);

        // Desativa o baú após o som tocar
        if (openChestClip != null)
            StartCoroutine(DeactivateAfterSound(openChestClip.length));
    }



    private IEnumerator DeactivateAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        // gameObject.SetActive(false);
    }
}

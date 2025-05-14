using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Vendedor interativo: exibe diálogo e, ao fechar, abre a loja.
/// </summary>
public class VendorInteractable : MonoBehaviour, IInteractable
{
    [Header("Diálogo de Boas-vindas")]
    [Tooltip("Caixa de diálogo (contém DialogueBox)")]
    public GameObject dialogueBox;

    [Tooltip("Linhas do diálogo antes de abrir a loja")]
    [TextArea(3, 8)]
    public string[] lines;

    [Tooltip("Velocidade de digitação, em segundos por caractere")]
    public float textSpeed = 0.05f;

    [Header("Configuração da Loja após o diálogo")]
    [Tooltip("Preço mínimo/máximo para os itens sorteados")]
    public int minPrice = 10;
    public int maxPrice = 50;
    [Tooltip("Itens possíveis à venda")]
    public ItemType[] possibleItems;

    private DialogueBox dlg;
    private bool        isDialogueActive = false;
    private PlayerController pc;

    void Start()
    {
        // Referência ao PlayerController
        pc = GameManager.thePlayer?.GetComponent<PlayerController>();

        // Inicializa diálogo
        if (dialogueBox == null)
        {
            Debug.LogError("VendorInteractable: atribua a dialogueBox no Inspector.");
            return;
        }
        dialogueBox.SetActive(false);

        dlg = dialogueBox.GetComponent<DialogueBox>();
        if (dlg == null)
        {
            Debug.LogError("VendorInteractable: DialogueBox não encontrado em dialogueBox.");
            return;
        }
        // Registra callback ao concluir o diálogo
        dlg.onComplete = OnDialogueComplete;
    }

    void Update()
    {
        // Fecha diálogo com a tecla de fechamento após exibir todas as linhas
        if (isDialogueActive && dlg != null && dlg.IsComplete && Input.GetKeyDown(dlg.closeKey))
        {
            CloseDialogue();
        }
    }

    /// <summary>
    /// Chamado pelo PlayerController.HandleInteraction() ao apertar E.
    /// </summary>
    public void Interact()
    {
        if (!isDialogueActive)
            ShowDialogue();
    }

    private void ShowDialogue()
    {
        // Trava movimento do player
        pc?.SetCanMove(false);

        dialogueBox.SetActive(true);
        isDialogueActive = true;
        dlg.StartDialog(lines, textSpeed);
    }

    private void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        isDialogueActive = false;

        // Após fechar diálogo, abre a loja
        OpenShop();
    }

    private void OnDialogueComplete()
    {
        // Opcional: mostrar prompt visual de "Pressione F para continuar".
    }

    private void OpenShop()
    {
        // Sorteia dois itens distintos
        var items = new List<ItemType>();
        if (possibleItems.Length > 0)
        {
            while (items.Count < 2)
            {
                var pick = possibleItems[UnityEngine.Random.Range(0, possibleItems.Length)];
                if (!items.Contains(pick))
                    items.Add(pick);
            }
        }

        // Sorteia preços
        var prices = new List<int>();
        foreach (var it in items)
            prices.Add(UnityEngine.Random.Range(minPrice, maxPrice + 1));

        // Abre a loja via ShopManager
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop(items, prices);
        }
        else
        {
            Debug.LogError("VendorInteractable: ShopManager.Instance é nulo.");
            // Se não houver ShopManager, libera movimento
            pc?.SetCanMove(true);
        }
    }
}

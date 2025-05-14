using System;
using UnityEngine;

public class SellerDialogue : MonoBehaviour, IInteractable
{
    [Tooltip("Caixa de diálogo (deve conter o componente DialogueBox)")]
    public GameObject dialogueBox;

    [Header("Linhas de diálogo do vendedor")]
    [TextArea(3,8)]
    public string[] lines;

    [Tooltip("Velocidade de digitação, em segundos por caractere")]
    public float textSpeed = 0.05f;

    private DialogueBox dlg;
    private bool        isDialogueActive = false;

    void Start()
    {
        if (dialogueBox == null)
        {
            Debug.LogError("SellerDialogue: atribua a DialogueBox no Inspector.");
            return;
        }

        dialogueBox.SetActive(false);
        dlg = dialogueBox.GetComponent<DialogueBox>();
        if (dlg == null)
        {
            Debug.LogError("SellerDialogue: DialogueBox não encontrado no objeto dialogueBox.");
            return;
        }

        // Callback opcional ao terminar todas as linhas
        dlg.onComplete = OnDialogueComplete;
    }

    // Chamado pelo PlayerController.HandleInteraction() quando o player aperta E
    public void Interact()
    {
        if (!isDialogueActive)
            ShowDialogue();
        else if (dlg.IsComplete)
            CloseDialogue();
    }

    private void ShowDialogue()
    {
        dialogueBox.SetActive(true);
        isDialogueActive = true;
        dlg.StartDialog(lines, textSpeed);
    }

    private void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        isDialogueActive = false;
    }

    private void OnDialogueComplete()
    {
        // Aqui você pode, se quiser, mostrar um prompt ("Pressione F para fechar")  
        // ou ativar botões de compra antes de permitir fechar.
    }
}
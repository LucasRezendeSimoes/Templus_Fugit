using System.Collections;
using UnityEngine;

/// <summary>
/// Exibe um diálogo de fragmento de memória, aplica tremor de câmera durante a interação
/// e coleta o item ao fechar.
/// </summary>
public class MemoryFragment : MonoBehaviour, IInteractable
{
    [Tooltip("Objeto da caixa de diálogo (contém DialogueBox)")]
    public GameObject dialogueBox;

    [Tooltip("Linhas de texto do fragmento de memória")]
    [TextArea(3, 6)]
    public string[] lines;

    [Tooltip("Velocidade de digitação, em segundos por caractere")]
    public float textSpeed = 0.05f;

    [Tooltip("Referência à câmera principal para o tremor")]
    public Camera mainCamera;

    private DialogueBox dlg;
    private bool isDialogueActive;
    private Coroutine cameraShakeCoroutine;

    void Start()
    {
        // Inicializa a caixa e o componente de diálogo
        dialogueBox.SetActive(false);
        dlg = dialogueBox.GetComponent<DialogueBox>();
        dlg.onComplete = OnDialogueComplete;
    }

    void Update()
    {
        // Permite fechar com a tecla configurada ao final do diálogo
        if (isDialogueActive && dlg.IsComplete && Input.GetKeyDown(dlg.closeKey))
        {
            CloseDialogue();
        }
    }

    // Chamado quando o jogador interage (tecla E, por exemplo)
    public void Interact()
    {
        if (!isDialogueActive)
            ShowDialogue();
    }

    private void ShowDialogue()
    {
        // Exibe diálogo e inicia digitação automática
        dialogueBox.SetActive(true);
        isDialogueActive = true;
        dlg.StartDialog(lines, textSpeed);

        // Inicia tremor contínuo na câmera
        if (mainCamera != null)
        {
            var cs = mainCamera.GetComponent<CameraShake>();
            if (cs != null)
                cameraShakeCoroutine = StartCoroutine(cs.ShakeContinuous(0.1f, 0.5f));
        }
    }

    private void CloseDialogue()
    {
        // Fecha diálogo
        dialogueBox.SetActive(false);
        isDialogueActive = false;

        // Para o tremor da câmera
        if (cameraShakeCoroutine != null && mainCamera != null)
        {
            var cs = mainCamera.GetComponent<CameraShake>();
            if (cs != null)
            {
                StopCoroutine(cameraShakeCoroutine);
                cs.StopShake();
            }
        }

        // Coleta o fragmento (não vai ao inventário)
        Destroy(gameObject);
    }

    private void OnDialogueComplete()
    {
        // Callback vazio (pode ser usado para UI adicional)
        
    }
}

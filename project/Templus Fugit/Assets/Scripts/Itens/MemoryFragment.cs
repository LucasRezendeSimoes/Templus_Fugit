using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryFragment : MonoBehaviour, IInteractable
{
    public GameObject dialogueBox; // Referência ao objeto do diálogo
    public Camera mainCamera; // Referência à câmera principal
    private bool isDialogueActive = false; // Verifica se o diálogo está ativo
    private Coroutine cameraShakeCoroutine; // Referência para o tremor da câmera

    // Start é chamado antes do primeiro frame update
    void Start()
    {
        dialogueBox.SetActive(false); // Garante que a caixa de diálogo esteja desativada no início
    }

    public void Interact()
    {
        if (!isDialogueActive)
        {
            ShowDialogue();
        }
        else
        {
            CloseDialogue();
        }
    }

    private void ShowDialogue()
    {
        dialogueBox.SetActive(true); // Ativa a caixa de diálogo
        isDialogueActive = true;

        // Certifique-se de que o texto do TMP não seja alterado
        var textComponent = dialogueBox.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.enabled = true; // Garante que o texto seja exibido
        }

        // Inicia o tremor da câmera
        if (mainCamera != null)
        {
            CameraShake cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake != null)
            {
                cameraShakeCoroutine = StartCoroutine(cameraShake.ShakeContinuous(0.1f, 0.5f)); // Tremor contínuo
            }
        }
    }

    private void CloseDialogue()
    {
        dialogueBox.SetActive(false); // Desativa a caixa de diálogo
        isDialogueActive = false;

        // Para o tremor da câmera
        if (cameraShakeCoroutine != null && mainCamera != null)
        {
            CameraShake cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake != null)
            {
                StopCoroutine(cameraShakeCoroutine); // Para o tremor contínuo
                cameraShake.StopShake(); // Garante que o tremor pare
            }
        }

        // Destroi o fragmento de memória
        Destroy(gameObject);
    }
}

using System;
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controla a exibição de linhas de texto com digitação automática e avançamento.
/// </summary>
public class DialogueBox : MonoBehaviour
{
    [Tooltip("Componente TextMeshProUGUI que mostrará o texto")]
    public TextMeshProUGUI textComponent;

    [Tooltip("Velocidade de digitação, em segundos por caractere")]
    public float textSpeed = 0.05f;

    [Tooltip("Tempo em segundos antes de avançar automaticamente à próxima linha")]
    public float autoNextDelay = 2f;

    [Tooltip("Tecla para fechar o diálogo ao final")]
    public KeyCode closeKey = KeyCode.F;

    /// <summary>Chamada quando todas as linhas forem exibidas.</summary>
    public Action onComplete;

    private string[] lines;
    private int index;
    private bool  isTyping;
    public bool   IsComplete { get; private set; }

    /// <summary>
    /// Inicia o diálogo com as linhas e velocidade configuradas.
    /// </summary>
    public void StartDialog(string[] dialogLines, float speed)
    {
        lines       = dialogLines;
        textSpeed   = speed;
        index       = 0;
        IsComplete  = false;
        textComponent.text = string.Empty;

        if (GameManager.Instance != null)
            GameManager.Instance.PauseTime();

        StartCoroutine(TypeLine());
    }


    void Update()
    {
        // Ao concluir todas as linhas, permite fechar com closeKey
        if (IsComplete && Input.GetKeyDown(closeKey))
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        textComponent.text = string.Empty;

        foreach (char c in lines[index])
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        // Avança automaticamente após delay
        yield return new WaitForSeconds(autoNextDelay);

        if (!isTyping)
        {
            NextOrComplete();
        }
    }

    private void NextOrComplete()
    {
        if (index < lines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            IsComplete = true;

            if (GameManager.Instance != null)
                GameManager.Instance.ResumeTime();

            onComplete?.Invoke();
        }
    }

}

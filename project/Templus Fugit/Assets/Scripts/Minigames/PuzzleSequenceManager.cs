using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gerencia o puzzle de sequência de teclas, com diálogo introdutório apenas em Cena5.
/// </summary>
public class PuzzleSequenceManager : MonoBehaviour
{
    [Header("Introdução ao Puzzle (Cena5)")]
    [Tooltip("Caixa de diálogo que contém o DialogueBox")]
    public GameObject dialogueBox;
    [Tooltip("Linhas explicando como funciona o puzzle")]
    [TextArea(3, 6)]
    public string[] dialogueLines;
    [Tooltip("Velocidade de digitação do diálogo (s por caractere)")]
    public float textSpeed = 0.05f;

    private DialogueBox dlg;
    private bool puzzleStarted = false;

    [Header("Sequência do Puzzle")]
    public List<KeyCode> sequence;
    private int currentIndex = 0;
    public TextMeshPro sequenceDisplay3D;
    public float penaltyTime = 5f;

    [Header("Referências e Dificuldade")]
    public PlayerController playerController;
    public Camera mainCamera;
    [Tooltip("Multiplicador de intensidade do tremor de câmera")]
    public float shakeIntensityFactor = 0.025f;
    [Tooltip("Fator para duração do tremor")]
    public float shakeDurationFactor = 0.5f;
    public int difficultyLevel = 1;

    private Coroutine cameraShakeCoroutine;
    private bool puzzleCompleted = false;

    void Start()
    {
        // Se estamos na Cena5, mostramos o diálogo antes de começar o puzzle
        if (SceneManager.GetActiveScene().name == "Cena5")
        {
            if (dialogueBox == null)
            {
                Debug.LogError("PuzzleSequenceManager: dialogueBox não atribuído.");
                BeginPuzzle();
            }
            else
            {
                dialogueBox.SetActive(false);
                dlg = dialogueBox.GetComponent<DialogueBox>();
                if (dlg == null)
                {
                    Debug.LogError("PuzzleSequenceManager: DialogueBox não encontrado em dialogueBox.");
                    BeginPuzzle();
                }
                else
                {
                    dlg.onComplete = OnDialogueComplete;
                    ShowDialogue();
                }
            }
        }
        else
        {
            BeginPuzzle();
        }
    }

    private void ShowDialogue()
    {
        // trava movimento do jogador durante o diálogo
        playerController?.SetCanMove(false);

        dialogueBox.SetActive(true);
        dlg.StartDialog(dialogueLines, textSpeed);
    }

    private void OnDialogueComplete()
    {
        dialogueBox.SetActive(false);
        BeginPuzzle();
    }

    private void BeginPuzzle()
    {
        puzzleStarted = true;
        currentIndex = 0;

        // inicia tremor de câmera contínuo
        if (mainCamera != null)
        {
            var cs = mainCamera.GetComponent<CameraShake>();
            if (cs != null)
            {
                float intensity = shakeIntensityFactor * difficultyLevel;
                float duration  = shakeDurationFactor / difficultyLevel;
                cameraShakeCoroutine = StartCoroutine(cs.ShakeContinuous(intensity, duration));
            }
        }

        // gera e exibe sequência
        GenerateRandomSequence(5 * difficultyLevel);
        UpdateSequenceText();

        // trava movimento até resolver o puzzle
        playerController?.SetCanMove(false);
    }

    void Update()
    {
        if (!puzzleStarted || puzzleCompleted) return;
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(sequence[currentIndex]))
            {
                StartCoroutine(FlashText(Color.green));
                currentIndex++;
                if (currentIndex >= sequence.Count)
                    PuzzleSolved();
            }
            else
            {
                GameManager.Instance.ReduceTime(penaltyTime);
                StartCoroutine(FlashText(Color.red));
                currentIndex = 0;
            }
        }
    }

    void UpdateSequenceText()
    {
        if (sequenceDisplay3D != null)
            sequenceDisplay3D.text = string.Join(" ", sequence.Select(k => k.ToString()));
    }

    IEnumerator FlashText(Color flashColor)
    {
        if (sequenceDisplay3D != null)
        {
            var orig = sequenceDisplay3D.color;
            sequenceDisplay3D.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            sequenceDisplay3D.color = orig;
        }
    }

    void PuzzleSolved()
    {
        puzzleCompleted = true;

        // para tremor de câmera
        if (cameraShakeCoroutine != null && mainCamera != null)
        {
            var cs = mainCamera.GetComponent<CameraShake>();
            if (cs != null)
            {
                StopCoroutine(cameraShakeCoroutine);
                cs.StopShake();
            }
        }

        // reativa movimento
        playerController?.SetCanMove(true);

        // abre portas
        foreach (var porta in GameObject.FindGameObjectsWithTag("Door"))
            Destroy(porta);

        // destrói o game object após completar o puzzle
        Destroy(gameObject);
    }

    void GenerateRandomSequence(int length)
    {
        KeyCode[] possibleKeys = {
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E,
            KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O,
            KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
        };
        sequence = possibleKeys.OrderBy(x => Random.value).Take(length).ToList();
    }
}

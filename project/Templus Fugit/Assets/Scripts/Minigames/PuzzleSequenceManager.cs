using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq; // Para usar métodos como OrderBy

public class PuzzleSequenceManager : MonoBehaviour
{
    public List<KeyCode> sequence;                // Sequência correta de teclas
    private int currentIndex = 0;

    public TextMeshPro sequenceDisplay3D;         // Texto 3D no chão
    public float penaltyTime = 5f;                // Tempo perdido ao errar

    public GameManager gameManager;               // Referência ao sistema de tempo
    private bool puzzleCompleted = false;

    public PlayerController playerController;     // Referência ao script de movimento do jogador

    public int difficultyLevel = 1;               // Nível de dificuldade (1 = fácil, aumenta com o tempo)

    public Camera mainCamera; // Referência à câmera principal
    private Coroutine cameraShakeCoroutine; // Referência para o tremor da câmera

    void Start()
    {
        // Inicia o tremor da câmera
        if (mainCamera != null)
        {
            CameraShake cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake != null)
            {
                float shakeIntensity = 0.025f * difficultyLevel; // Intensidade aumenta com a dificuldade
                float shakeDuration = 0.5f / difficultyLevel;  // Duração diminui com a dificuldade
                cameraShakeCoroutine = StartCoroutine(cameraShake.ShakeContinuous(shakeIntensity, shakeDuration)); // Tremor contínuo
            }
        }
        GenerateRandomSequence(5 * difficultyLevel); // Gera uma sequência baseada na dificuldade
        UpdateSequenceText();

        // Travar o movimento do jogador ao iniciar o puzzle
        if (playerController != null)
        {
            playerController.SetCanMove(false); // Impede o movimento
        }
    }

    void Update()
    {
        if (puzzleCompleted) return;

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
                {
                    PuzzleSolved();
                }
            }
            else
            {
                // Penalidade
                GameManager.Instance.ReduceTime(penaltyTime); // Chama o método diretamente no GameManager
                StartCoroutine(FlashText(Color.red));
                currentIndex = 0;
            }
        }
    }

    void UpdateSequenceText()
    {
        string display = "";
        foreach (KeyCode key in sequence)
        {
            display += key.ToString() + " ";
        }

        if (sequenceDisplay3D != null)
        {
            sequenceDisplay3D.text = display;
        }
    }

    IEnumerator FlashText(Color flashColor)
    {
        if (sequenceDisplay3D != null)
        {
            Color originalColor = sequenceDisplay3D.color;
            sequenceDisplay3D.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            sequenceDisplay3D.color = originalColor;
        }
    }

    void PuzzleSolved()
    {
        puzzleCompleted = true;

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

        // Reativar o movimento do jogador ao resolver o puzzle
        if (playerController != null)
        {
            playerController.SetCanMove(true); // Permite o movimento
        }

        // Abrir portas (destruir objetos com tag "Porta")
        GameObject[] portas = GameObject.FindGameObjectsWithTag("Door");
        foreach (GameObject porta in portas)
        {
            Destroy(porta);
        }

        // Destruir o objeto atual (o que contém o script)
        Destroy(gameObject);
    }

    void GenerateRandomSequence(int length)
    {
        // Lista de teclas possíveis
        KeyCode[] possibleKeys = {
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E,
            KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O,
            KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
        };

        // Embaralha e seleciona as primeiras 'length' teclas
        sequence = possibleKeys.OrderBy(x => Random.value).Take(length).ToList();
    }
}

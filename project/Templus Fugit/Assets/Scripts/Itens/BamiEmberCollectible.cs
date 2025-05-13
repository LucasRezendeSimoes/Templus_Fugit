using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BamiEmberCollectible : MonoBehaviour
{
    [Tooltip("Prefab da Flame Ball registrado no GameManager")]
    public GameObject flameBallPrefab;

    void Awake()
    {
        // Garante que o collider seja trigger para OnTriggerEnter2D
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
        else
            Debug.LogWarning("BamiEmberCollectible: nenhum Collider2D encontrado no GameObject.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Só coleta se for o jogador
        if (!other.CompareTag("Player"))
            return;

        // Concede o Flame Power e incrementa o contador de brasas
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GrantFlamePower(flameBallPrefab);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("GameManager.Instance é null ao coletar Brasa de Bami.");
        }
    }
}

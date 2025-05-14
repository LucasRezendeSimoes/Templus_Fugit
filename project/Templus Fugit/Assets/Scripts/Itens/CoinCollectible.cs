using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    void Awake()
    {
        // garante que seja trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // adiciona 1 moeda e destrói este pickup
        GameManager.Instance.AddCoins(1);
        Destroy(gameObject);
    }
}

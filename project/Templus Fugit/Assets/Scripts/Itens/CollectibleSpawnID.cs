using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawnID : MonoBehaviour
{
    [HideInInspector] public string spawnID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Marca este spawn como coletado nesta run
        GameManager.Instance.RegisterCollectedSpawn(spawnID);

        // Depois, deixa o coletável executar seu comportamento normal (ex: Destroy)
    }
}

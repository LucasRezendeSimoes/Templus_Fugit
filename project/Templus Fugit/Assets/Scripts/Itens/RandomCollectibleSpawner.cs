using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCollectibleSpawner : MonoBehaviour
{
    [Tooltip("ID único deste ponto de spawn (ex: 'Scene4_Item1')")]
    public string spawnID;

    [Tooltip("Prefabs de todos os itens coletáveis do jogo")]
    public GameObject[] collectiblePrefabs;

    [Tooltip("Ponto de spawn. Se vazio, usa a posição deste GameObject")]
    public Transform spawnPoint;

    void Start()
    {
        // 1) Se já coletaram nesta run, não instanciamos nada
        if (GameManager.Instance.collectedSpawnIDs.Contains(spawnID))
            return;

        // 2) Sorteamos um dos prefabs
        int idx = Random.Range(0, collectiblePrefabs.Length);
        var prefab = collectiblePrefabs[idx];
        if (prefab == null)
        {
            Debug.LogWarning($"Spawner '{spawnID}': prefab[{idx}] é nulo.");
            return;
        }

        // 3) Escolhe o local de instanciação
        Vector3 pos = (spawnPoint != null ? spawnPoint.position : transform.position);
        Quaternion rot = prefab.transform.rotation;

        // 4) Instancia o item no chão
        var go = Instantiate(prefab, pos, rot);

        // 5) Adiciona o script que registra a coleta
        var tracker = go.AddComponent<CollectibleSpawnID>();
        tracker.spawnID = spawnID;
    }
}

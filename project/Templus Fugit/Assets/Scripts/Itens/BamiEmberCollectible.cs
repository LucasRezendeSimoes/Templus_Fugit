using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BamiEmberCollectible : MonoBehaviour
{
    public GameObject flameBallPrefab; // prefab da bola de fogo

    private void Awake()
    {
        // garante que só dispare trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // se conseguiu colocar no inventário, destrói só este pick-up
        if (GameManager.Instance.AddInventoryItem(ItemType.BamiEmber))
        {
            GameManager.Instance.GrantFlamePower(flameBallPrefab);
            Destroy(gameObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class VendorInteractable : MonoBehaviour, IInteractable
{
    [Header("Preço mínimo/máximo")]
    public int       minPrice      = 10;
    public int       maxPrice      = 50;
    public ItemType[] possibleItems;

    public void Interact()
    {
        var pc = GameManager.thePlayer?.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        // Sorteia dois itens distintos
        var items = new List<ItemType>();
        if (possibleItems.Length > 0)
        {
            while (items.Count < 2)
            {
                var pick = possibleItems[Random.Range(0, possibleItems.Length)];
                if (!items.Contains(pick))
                    items.Add(pick);
            }
        }

        // Sorteia preços
        var prices = new List<int>();
        foreach (var it in items)
            prices.Add(Random.Range(minPrice, maxPrice + 1));

        // Abre a loja
        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenShop(items, prices);
        else
        {
            Debug.LogError("ShopManager.Instance é nulo! Verifique se existe um ShopManager ativo na cena.");
            // reativa o player imediatamente
            pc?.SetCanMove(true);
        }
    }
}

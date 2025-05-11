using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuração do Vendedor")]
    public int minPrice = 10;
    public int maxPrice = 50;
    public ItemType[] possibleItems;  // itens que podem aparecer à venda

    public void Interact()
    {
        // 1) trava o movimento do player
        var pc = GameManager.thePlayer.GetComponent<PlayerController>();
        if (pc != null) 
            pc.SetCanMove(false);

        // 2) sorteia 2 itens distintos
        var items = new List<ItemType>();
        while (items.Count < 2)
        {
            var candidate = possibleItems[Random.Range(0, possibleItems.Length)];
            if (!items.Contains(candidate))
                items.Add(candidate);
        }

        // 3) sorteia um preço para cada
        var prices = new List<int>();
        foreach (var it in items)
            prices.Add(Random.Range(minPrice, maxPrice + 1));

        // 4) abre a loja via singleton
        ShopManager.Instance.OpenShop(items, prices);
    }
}

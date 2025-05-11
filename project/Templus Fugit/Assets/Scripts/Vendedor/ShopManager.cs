using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Serializable]
    public struct SlotUI
    {
        public Button    buyButton;
        public Image     icon;
        public TextMeshProUGUI priceText;
    }

    public SlotUI[] slots;                // Array de tamanho 2
    public Button closeButton;

    private GameObject shopPanelRoot;
    private List<ItemType> shopItems;     // os 2 itens sorteados
    private int[]          prices;        // preço de cada um

    public static ShopManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        shopPanelRoot = transform.parent != null 
            ? transform.parent.gameObject 
            : gameObject;
        
        // shopPanelRoot = gameObject;
        shopPanelRoot.SetActive(false);
        closeButton.onClick.AddListener(CloseShop);
    }

    public void OpenShop(List<ItemType> items, List<int> itemPrices)
    {
        shopItems = items;
        prices    = itemPrices.ToArray();
        shopPanelRoot.SetActive(true);

        for (int i = 0; i < slots.Length; i++)
        {
            var it = shopItems[i];
            slots[i].icon.sprite    = GameManager.Instance.GetItemIcon(it);
            slots[i].priceText.text = prices[i].ToString();
            int idx = i;
            slots[i].buyButton.onClick.RemoveAllListeners();
            slots[i].buyButton.onClick.AddListener(() => TryBuy(idx));
        }
    }

    private void TryBuy(int index)
    {
        int cost = prices[index];
        if (GameManager.Instance.coinCount >= cost)
        {
            // desconta moedas
            GameManager.Instance.AddCoins(-cost);
            // adiciona o item ao inventário
            GameManager.Instance.AddInventoryItem(shopItems[index]);
            CloseShop();
        }
        else
        {
            // feedback: moedas insuficientes
            Debug.Log("Você não tem moedas suficientes!");
        }
    }

    private void CloseShop()
    {
        shopPanelRoot.SetActive(false);
        // Reativa movimentação do jogador
        PlayerController pc = GameManager.thePlayer.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(true);
    }
}

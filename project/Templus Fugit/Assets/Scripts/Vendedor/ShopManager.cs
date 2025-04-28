using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Serializable]
    public struct SlotUI
    {
        public Button           buyButton;
        public Image            icon;
        public TextMeshProUGUI  priceText;
    }

    [Header("Atribua aqui no Inspector")]
    public GameObject shopPanelRoot;  // arraste o GameObject ShopPanel
    public Button     closeButton;
    public SlotUI[]   slots;          // tamanho = 2

    private List<ItemType> shopItems;
    private int[]          prices;

    void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Escondemos o painel até o momento de abrir a loja
        if (shopPanelRoot == null)
            Debug.LogError("ShopPanelRoot não foi atribuído no ShopManager!");
        else
            shopPanelRoot.SetActive(false);

        closeButton.onClick.AddListener(CloseShop);
    }

    public void OpenShop(List<ItemType> items, List<int> itemPrices)
    {
        shopItems = items;
        prices    = itemPrices.ToArray();

        // Mostra o painel
        shopPanelRoot.SetActive(true);

        // Preenche os slots
        for (int i = 0; i < slots.Length; i++)
        {
            var it = shopItems[i];
            slots[i].icon.sprite    = GameManager.Instance.GetItemIcon(it);
            slots[i].priceText.text = prices[i].ToString();

            int idx = i; // evita captura de variável
            slots[i].buyButton.onClick.RemoveAllListeners();
            slots[i].buyButton.onClick.AddListener(() => TryBuy(idx));
        }
    }

    private void TryBuy(int index)
    {
        int cost = prices[index];
        if (GameManager.Instance.coinCount >= cost)
        {
            GameManager.Instance.AddCoins(-cost);
            GameManager.Instance.AddInventoryItem(shopItems[index]);
            CloseShop();
        }
        else
        {
            Debug.Log("Você não tem moedas suficientes!");
        }
    }

    private void CloseShop()
    {
        shopPanelRoot.SetActive(false);
        var pc = GameManager.thePlayer?.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(true);
    }
}
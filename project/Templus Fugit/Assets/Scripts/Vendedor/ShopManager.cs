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

    [Header("Referências da UI")]
    [Tooltip("Arraste aqui o painel root da loja (ShopPanel)")]
    public GameObject      shopPanelRoot;
    public SlotUI[]        slots;        // tamanho = 2

    [Header("Flame Ball Prefab")]
    [Tooltip("Arraste aqui o prefab da FlameBall usado no GameManager")]
    public GameObject      flameBallPrefab;

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

        if (shopPanelRoot == null)
            Debug.LogError("ShopPanelRoot não foi atribuído no ShopManager!");
        else
            shopPanelRoot.SetActive(false);
    }

    /// <summary>
    /// Abre a loja com os itens e preços sorteados.
    /// </summary>
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

    void Update()
    {
        // Fechar a loja ao pressionar a tecla "ESQ" (Esc)
        if (shopPanelRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    /// <summary>
    /// Tenta comprar o item no slot indicado.
    /// Para BamiEmber, já aplica o poder imediatamente.
    /// </summary>
    private void TryBuy(int index)
    {
        var item = shopItems[index];
        int cost = prices[index];

        if (GameManager.Instance.coinCount < cost)
        {
            Debug.Log("Você não tem moedas suficientes!");
            return;
        }

        // Desconta moedas
        GameManager.Instance.AddCoins(-cost);

        if (item == ItemType.BamiEmber)
        {
            // Concede o poder da flame ball imediatamente
            if (flameBallPrefab != null)
            {
                GameManager.Instance.AddInventoryItem(item);
                GameManager.Instance.GrantFlamePower(flameBallPrefab);
            }
            else
                Debug.LogWarning("FlameBallPrefab não atribuído no ShopManager.");
        }
        else
        {
            // Adiciona ao inventário normalmente
            GameManager.Instance.AddInventoryItem(item);
        }

        CloseShop();
    }

    /// <summary>
    /// Fecha a janela da loja e reativa movimentação.
    /// </summary>
    private void CloseShop()
    {
        shopPanelRoot.SetActive(false);
        var pc = GameManager.thePlayer?.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(true);
    }
}
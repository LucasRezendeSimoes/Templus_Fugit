using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vendedorController : MonoBehaviour
{

    public GameObject[] storePool; //Pool de itens que podem aparecer na loja (colocar os prefabs dos itens no editor)
    public Transform[] itemSlots; // posi��es no cen�rio onde os itens v�o aparecer

    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> embaralhado = new List<GameObject>(storePool);// Cria uma lista baseada no array storePool para poder embaralhar
        EmbaralharLista(embaralhado);// Embaralha a lista para randomizar os itens

        int itensParaMostrar = Mathf.Min(itemSlots.Length, embaralhado.Count); // Define o n�mero de itens que vamos mostrar (o m�nimo entre slots dispon�veis e itens embaralhados)

        for (int i = 0; i < itensParaMostrar; i++) // Instancia os itens embaralhados nos slots da loja
        {
            GameObject item = Instantiate(embaralhado[i], itemSlots[i].position, Quaternion.identity);// Cria uma c�pia do item no slot correspondente
            item.transform.SetParent(itemSlots[i]); // Define o slot como "pai" do item para organizar a hierarquia na cena (opcional)

            // Atribui um valor aleat�rio entre 10 e 100, por exemplo
            int precoAleatorio = Random.Range(10, 20);

            StoreItem itemScript = item.GetComponent<StoreItem>();
            if (itemScript != null)
            {
                itemScript.DefinirPreco(precoAleatorio);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(GameManager.PlayerScore1);
    }

    void EmbaralharLista(List<GameObject> lista)    // Fun��o que embaralha a lista usando algoritmo de Fisher-Yates (shuffle)

    {
        for (int i = 0; i < lista.Count; i++)
        {
            int randomIndex = Random.Range(i, lista.Count);  // Escolhe um �ndice aleat�rio entre o �ndice atual e o final da lista

            // Troca o item atual com o item sorteado
            GameObject temp = lista[i];
            lista[i] = lista[randomIndex];
            lista[randomIndex] = temp;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum ItemType 
{
    None = 0,
    HealthPotion = 1,
    CloakNyx      = 2,
    Hourglass     = 3,
    BrokenWatch   = 4,
    BamiEmber     = 5, 
    // OracleEye     = 6,
}

public class GameManager : MonoBehaviour
{
    [Tooltip("Arraste aqui o prefab do HUD")]
    public GameObject hudPrefab;
    private static bool _hudCreated = false;

    // references to HUD elements
    private Image   _coinIconUI;
    private TextMeshProUGUI _coinTextUI;
    private TextMeshProUGUI _timeTextUI;

    public static GameManager Instance;       // Singleton instance
    public static int PlayerScore1 = 0;       // Pontuação do player 1
    public GUISkin layout;                    // Fonte do contador de tempo
    public static GameObject thePlayer;       // Referência ao objeto jogador

    [Header("Player Settings")]
    public static int lifes = 3;              // Vidas do jogador
    public int maxLifes = 3;                  // Vidas máximas do jogador

    public float gameTime = 300f;             // Tempo total do jogo em segundos
    private float currentTime;                // Tempo restante

    [Header("Inventário")]
    public int inventorySize = 5;             // quantos slots
    public Sprite emptySlotSprite;            // sprite slot vazio
    public Sprite[] itemIcons;                // ícones p/ cada ItemType (índice = enum)
    private ItemType[] inventory;             // guarda o que o player carrega
    private Image[]   inventorySlotImages;    // referências aos 5 Image slots

    [Header("Ampulheta")]
    public float hourglassBonusTime = 30f;    // tempo extra do Hourglass

    [Header("Relogio Quebrado")]
    public float brokenWatchDuration = 10f;
    public bool IsTimeStopped { get; private set; }

    [Header("Véu de Nyx")]
    public float cloakDuration = 5f;
    // para que inimigos possam checar se o player está invisível
    public bool IsInvisible { get; private set; }

    [Header("Bola de Fogo")]
    private bool _hasFlamePower = false;
    private GameObject _flameBallPrefab;
    public bool CanUseFlame => _hasFlamePower;
    public GameObject FlameBallPrefab => _flameBallPrefab;
    [SerializeField] private Sprite flameBallIcon;       // atribuir no Inspector
    private Image _powerIconUI;
    private Image[] _cronosSlotImages;
    [Tooltip("Dano base da Flame Ball")]
    [SerializeField] private int baseFlameDamage = 10;
    [Tooltip("Dano adicional por Brasa de Bami")]
    [SerializeField] private int damagePerEmber = 10;
    public int bamiEmberCount = 0;
    
    [Header("Moedas")]
    public int coinCount = 0;

    [Header("HUD de Vidas")]
    public Image[] heartsUI;  // Arraste aqui seus 3 objetos heart_0, heart_1 e heart_2 (UI Images)

    [Header("Soundtrack")]
    [Tooltip("Arraste aqui as 3 músicas na ordem que devem tocar")]
    public AudioClip[] soundtrack;
    [Tooltip("Tempo em segundos para cross‐fade entre faixas")]
    public float crossfadeDuration = 2f;

    // componentes internos
    private AudioSource[] _musicSources = new AudioSource[2];
    private int           _activeSource;
    private int           _nextClipIndex;

    // Dicionário para armazenar a última posição do jogador em cada cena
    private Dictionary<string, Vector3?> savedPositions = new Dictionary<string, Vector3?>();

    // Variável para armazenar a cena anterior
    private string previousScene = null;

    private bool isInteracting = false; // Verifica se o jogador está interagindo com algo

    private readonly Dictionary<(string from, string to), Vector3> sceneTransitions = new Dictionary<(string, string), Vector3>
    {
        { ("Cena3", "Cena2"), new Vector3(-6.042691f, 8.385761f, 0f) },
        { ("Cena2", "Cena3"), new Vector3(6.16f, -1.6f, 0f) },
        { ("Cena4", "Cena2"), new Vector3(5.78788f, 8.521598f, 0f) },
        { ("Cena2", "Cena4"), new Vector3(-6.16f, -1.03f, 0f) },
        { ("Cena5", "Cena2"), new Vector3(-0.08580431f, 14.966f, 0f) },
        { ("Cena2", "Cena5"), new Vector3(0.02f, -2.710258f, 0f) },
        { ("Cena10", "Cena5"), new Vector3(-6.265987f, -0.5999068f, 0f) },
        { ("Cena5", "Cena10"), new Vector3(6.02f, -0.66f, 0f) },
        { ("Cena9", "Cena5"), new Vector3(6.159083f, -0.532959f, 0f) },
        { ("Cena5", "Cena9"), new Vector3(-6.272023f, -0.66f, 0f) },
        { ("Cena6", "Cena5"), new Vector3(0.02f, 1.730677f, 0f) },
        { ("Cena5", "Cena6"), new Vector3(0.02f, -2.7f, 0f) },
        { ("Cena15", "Cena6"), new Vector3(0.02f, 1.730677f, 0f) },
        { ("Cena6", "Cena15"), new Vector3(0.02f, -2.7f, 0f) },
        { ("Cena11", "Cena10"), new Vector3(0.02f, 1.730677f, 0f) },
        { ("Cena10", "Cena11"), new Vector3(0.02f, -2.7f, 0f) },
        { ("Cena12", "Cena9"), new Vector3(0.02f, 1.730677f, 0f) },
        { ("Cena9", "Cena12"), new Vector3(0.02f, -2.7f, 0f) },
        { ("Cena13", "Cena11"), new Vector3(0.02f, 1.730677f, 0f) },
        { ("Cena11", "Cena13"), new Vector3(0.02f, -2.7f, 0f) },
        { ("Cena14", "Cena12"), new Vector3(0.02f, 1.730677f, 0f) },
        { ("Cena12", "Cena14"), new Vector3(0.02f, -2.7f, 0f) },
        { ("Cena15", "Cena13"), new Vector3(6.105287f, -0.4260389f, 0f) },
        { ("Cena13", "Cena15"), new Vector3(-5.948434f, -0.519862f, 0f) },
        { ("Cena14", "Cena15"), new Vector3(5.942899f, -0.4849414f, 0f) },
        { ("Cena15", "Cena14"), new Vector3(-6.119143f, -0.6036257f, 0f) },
        { ("Cena15", "Cena16"), new Vector3(0.02f, -2.7f, 0f) }
    };

    private readonly List<string> sceneOrder = new List<string>
    {
        "Cena1", "Cena2", "Cena3", "Cena4", "Cena5", "Cena6", "Cena7", "Cena8",
        "Cena9", "Cena10", "Cena11", "Cena12", "Cena13", "Cena14", "Cena15", "Cena16_Boss"
    };

    public HashSet<string> openedChests = new HashSet<string>();

    public HashSet<string> collectedSpawnIDs = new HashSet<string>();


    void Awake()
    {
         // --- aplica volume salvo antes de tocar qualquer coisa ---
        float savedVol = PlayerPrefs.GetFloat("musicVolume", 1f);
        AudioListener.volume = savedVol;
        // Singleton padrão
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- SETUP DO AUDIO ---
            for (int i = 0; i < 2; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop        = false;
                src.volume      = (i == 0) ? 1f : 0f;
                _musicSources[i] = src;
            }

            if (soundtrack != null && soundtrack.Length > 0)
            {
                // toca a primeira faixa
                _musicSources[0].clip = soundtrack[0];
                _musicSources[0].Play();
                _nextClipIndex = 1 % soundtrack.Length;
                StartCoroutine(PlaylistCoroutine());
            }
            // --- FIM SETUP AUDIO ---

            // Instancia o HUD apenas 1x
            if (!_hudCreated && hudPrefab != null)
            {
                GameObject hud = Instantiate(hudPrefab);
                DontDestroyOnLoad(hud);
                _hudCreated = true;

                // pega o TextMeshProUGUI chamado "TimeText"
                var timeTextTransform = hud.transform
                                         .Find("HUD/TimeText");
                if (timeTextTransform != null)
                    _timeTextUI = timeTextTransform.GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogError("Não achei HUD/TimeText no prefab de HUD!");

                // capturando a TextMeshProUGUI do contador de moedas —————
                var coinsTextTransform = hud.transform
                                           .Find("HUD/CoinPanel/CoinsText");
                if (coinsTextTransform != null)
                    _coinTextUI = coinsTextTransform
                                    .GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogError("GameManager: não achei HUD/CoinPanel/CoinsText!");

                // Se quiser exibir também um ícone:
                var coinIconTransform = hud.transform
                                          .Find("HUD/CoinPanel/CoinIcon");
                if (coinIconTransform != null)
                    _coinIconUI = coinIconTransform
                                    .GetComponent<Image>();

                // Agora buscamos todas as Images cujo nome contenha "heart"
                var heartImages = new List<Image>();
                foreach (var img in hud.GetComponentsInChildren<Image>(true))
                {
                    // Ajuste esse filtro ao nome exato dos seus GameObjects
                    if (img.gameObject.name.ToLower().Contains("heart"))
                        heartImages.Add(img);
                }

                // Ordene, se quiser, por nome (“heart_0”, “heart_1”, “heart_2”)
                heartImages.Sort((a,b)=>
                    a.gameObject.name.CompareTo(b.gameObject.name)
                );

                heartsUI = heartImages.ToArray();

                // Debug caso não tenha encontrado
                if (heartsUI.Length == 0)
                    Debug.LogError("GameManager: não encontrei nenhuma Image com 'heart' no nome dentro do HUD!");

                // inicializa arrays
                inventory        = new ItemType[inventorySize];
                inventorySlotImages = new Image[inventorySize];

                // encontra o painel de slots
                var invPanel = hud.transform.Find("HUD/InventoryPanel");
                if (invPanel == null) {
                    Debug.LogError("GameManager: não achei HUD/InventoryPanel no prefab!");
                } else {
                    for (int i = 0; i < inventorySize; i++) {
                        Transform slotTf = invPanel.Find($"Slot_{i}");
                        if (slotTf == null) {
                            Debug.LogWarning($"GameManager: não achei Slot_{i} em InventoryPanel");
                            continue;
                        }

                        // pega a própria Image do Slot_X
                        Image slotImg = slotTf.GetComponent<Image>();
                        if (slotImg == null) {
                            Debug.LogWarning($"GameManager: Slot_{i} não tem Image!");
                            continue;
                        }

                        inventorySlotImages[i] = slotImg;

                        // inicializa vazio:
                        inventory[i] = ItemType.None;
                        slotImg.sprite = emptySlotSprite;
                    }
                }
                var powerIconTf = hud.transform.Find("HUD/PowerPanel/PowerIcon");
                if (powerIconTf != null)
                    _powerIconUI = powerIconTf.GetComponent<Image>();
                else
                    Debug.LogError("GameManager: não achei HUD/PowerPanel/PowerIcon no prefab!");
                // // procura o CronosPowerPanel
                // var cronosPanelTf = hud.transform.Find("HUD/CronosPowerPanel");
                // if (cronosPanelTf == null)
                //     Debug.LogError("GameManager: não achei HUD/CronosPowerPanel!");
                // else {
                //     _cronosSlotImages = new Image[2];
                //     for (int i = 0; i < 2; i++) {
                //         // cada slot tem a hierarquia CronosPowerPanel/Slot_{i+1}/Panel
                //         var panel = cronosPanelTf.Find($"Slot_{i+1}/Panel");
                //         if (panel != null)
                //             _cronosSlotImages[i] = panel.GetComponent<Image>();
                //         else
                //             Debug.LogWarning($"GameManager: faltando Slot_{i+1}/Panel em CronosPowerPanel");
                //         // inicializa vazio
                //         if (_cronosSlotImages[i] != null)
                //             _cronosSlotImages[i].sprite = emptySlotSprite;
                //     }
                // }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator PlaylistCoroutine()
    {
        while (true)
        {
            var active   = _musicSources[_activeSource];
            float remain = active.clip.length - active.time;

            // espera até faltar exatamente crossfadeDuration
            yield return new WaitForSeconds(Mathf.Max(0, remain - crossfadeDuration));

            // prepara o outro source
            int nextSrcIdx   = 1 - _activeSource;
            var incoming     = _musicSources[nextSrcIdx];
            incoming.clip    = soundtrack[_nextClipIndex];
            incoming.time    = 0f;
            incoming.volume  = 0f;
            incoming.Play();

            // faz o fade entre os dois
            float t = 0f;
            while (t < crossfadeDuration)
            {
                t += Time.deltaTime;
                float f = t / crossfadeDuration;
                active.volume   = Mathf.Lerp(1f, 0f, f);
                incoming.volume = Mathf.Lerp(0f, 1f, f);
                yield return null;
            }

            // finalize
            active.Stop();
            active.volume      = 1f;
            incoming.volume    = 1f;

            // avança índices
            _activeSource    = nextSrcIdx;
            _nextClipIndex   = (_nextClipIndex + 1) % soundtrack.Length;
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded; // Registra o evento de carregamento de cena
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded; // Remove o evento de carregamento de cena

    // Chamado quando uma nova cena é carregada
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Atualiza a referência ao jogador
        thePlayer = GameObject.FindGameObjectWithTag("Player");

        // Verifica se há uma cena anterior registrada
        if (!string.IsNullOrEmpty(previousScene))
        {
            Debug.Log($"Transição de cena: {previousScene} -> {scene.name}");
        }

        // Posiciona o jogador na cena carregada
        PositionPlayerOnSceneLoad();

        // Atualiza a variável da cena anterior após a lógica de transição
        previousScene = scene.name;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (currentTime <= 0) currentTime = gameTime;
        thePlayer = GameObject.FindGameObjectWithTag("Player");
        UpdateHeartsUI();
        PositionPlayerOnSceneLoad();
        UpdateCoinsUI();
        UpdateTimeUI();
    }   

    // Salva a posição atual do jogador no dicionário
    private void SavePlayerPosition()
    {
        if (thePlayer != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            savedPositions[currentScene] = thePlayer.transform.position;
        }
    }

    // Posiciona o jogador ao carregar uma nova cena
    private void PositionPlayerOnSceneLoad()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Cena atual: {currentScene}, Cena anterior: {previousScene}");

        if (savedPositions.ContainsKey(currentScene) && savedPositions[currentScene].HasValue)
        {
            if (sceneTransitions.TryGetValue((previousScene, currentScene), out Vector3 targetPosition))
            {
                thePlayer.transform.position = targetPosition;
            }
            else
            {
                thePlayer.transform.position = savedPositions[currentScene].Value;
            }
        }
        else
        {
            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint != null)
            {
                thePlayer.transform.position = spawnPoint.transform.position;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Impede a transição de cena enquanto o jogador está interagindo
        if (isInteracting) return;

        if (thePlayer == null)
        {
            thePlayer = GameObject.FindGameObjectWithTag("Player");
            if (thePlayer == null)
            {
                return; // ainda não está disponível, evita NullReference
            }
        }

        UpdateHeartsUI();
        UpdateCoinsUI();
        UpdateTimeUI();

        // Verifica se a cena atual é "Cena2" ou posterior antes de atualizar o tempo
        if (!IsTimeStopped && (SceneManager.GetActiveScene().name == "Cena2" || IsSceneAfter("Cena2")))
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
                RestartGame();
        }

        CheckSceneTransitions(SceneManager.GetActiveScene().name, thePlayer.transform.position);
    }

    private void CheckSceneTransitions(string scene, Vector2 pos)
    {
        var transitions = new Dictionary<string, (Vector2 min, Vector2 max, string targetScene)[]>
        {
            { "Cena1", new[] { (new Vector2(-1.909972f, 34.06695f), new Vector2(1.910001f, 34.06695f), "Cena2") } },
            { "Cena2", new[] { (new Vector2(-0.9350259f, 15.52005f), new Vector2(0.935003f, 15.52005f), "Cena5"),
                               (new Vector2(-6.335011f, 8.068845f), new Vector2(-6.335011f, 8.935001f), "Cena3"),
                               (new Vector2(6.308114f, 8.064985f), new Vector2(6.308114f, 8.935001f), "Cena4") } },
            { "Cena3", new[] { (new Vector2(6.472393f, -1.251016f), new Vector2(6.472393f, -0.2910173f), "Cena2") } },
            { "Cena4", new[] { (new Vector2(-6.632596f, -1.532009f), new Vector2(-6.632596f, -0.4679966f), "Cena2") } },
            { "Cena5", new[] { (new Vector2(-0.7820935f, -3.702741f), new Vector2(0.8307027f, -3.702741f), "Cena2"),
                               (new Vector2(-0.7181748f, 2.066446f), new Vector2(0.7148368f, 2.066446f), "Cena6"),
                               (new Vector2(6.514736f, -1.51467f), new Vector2(6.514736f, 0.5432134f), "Cena9"),
                               (new Vector2(-6.772685f, -1.605973f), new Vector2(-6.772685f, 0.5947051f), "Cena10") } },
            { "Cena6", new[] { (new Vector2(-0.7820935f, -3.11787f), new Vector2(0.8307027f, -3.11787f), "Cena5"),
                               (new Vector2(-0.7684372f, 2.016829f), new Vector2(0.760188f, 2.016829f), "Cena15") } },
            { "Cena8", new[] { (new Vector2(-0.7060629f, -3.107711f), new Vector2(0.7072755f, -3.107711f), "Cena5") } },
            { "Cena9", new[] { (new Vector2(-6.678032f, -1.55167f), new Vector2(-6.678032f, 0.5063582f), "Cena5"),
                               (new Vector2(-0.7181748f, 2.003242f), new Vector2(0.7148368f, 2.003242f), "Cena12") } },
            { "Cena10", new[] { (new Vector2(6.734772f, -1.521577f), new Vector2(6.734772f, 0.5304125f), "Cena5"),
                                (new Vector2(-0.7181748f, 2.003242f), new Vector2(0.7148368f, 2.003242f), "Cena11") } },
            { "Cena11", new[] { (new Vector2(-0.7820935f, -3.11787f), new Vector2(0.8307027f, -3.11787f), "Cena10"),
                                (new Vector2(-0.7181748f, 2.003242f), new Vector2(0.7148368f, 2.003242f), "Cena13") } },
            { "Cena12", new[] { (new Vector2(-0.7820935f, -3.11787f), new Vector2(0.8307027f, -3.11787f), "Cena9"),
                                (new Vector2(-0.7181748f, 2.003242f), new Vector2(0.7148368f, 2.003242f), "Cena14") } },
            { "Cena13", new[] { (new Vector2(-0.7820935f, -3.11787f), new Vector2(0.8307027f, -3.11787f), "Cena11"),
                                (new Vector2(6.600544f, -1.586784f), new Vector2(6.600544f, 0.5661189f), "Cena15") } },
            { "Cena14", new[] { (new Vector2(-0.7820935f, -3.11787f), new Vector2(0.8307027f, -3.11787f), "Cena12"),
                                (new Vector2(-6.746841f, -1.55167f), new Vector2(-6.746841f, 0.5063582f), "Cena15") } },
            { "Cena15", new[] { (new Vector2(-0.7820935f, -3.100172f), new Vector2(0.8307027f, -3.100172f), "Cena6"),
                                (new Vector2(-6.678032f, -1.55167f), new Vector2(-6.678032f, 0.5063582f), "Cena13"),
                                (new Vector2(6.600544f, -1.568303f), new Vector2(6.600544f, 0.5683065f), "Cena14"),
                                (new Vector2(-0.7296513f, 2.066446f), new Vector2(0.7507334f, 2.066446f), "Cena16_Boss") } }
        };

        if (transitions.ContainsKey(scene))
        {
            foreach (var (min, max, targetScene) in transitions[scene])
            {
                if (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y)
                {
                    // Verifica se a cena de destino é diferente da cena atual
                    if (SceneManager.GetActiveScene().name != targetScene)
                    {
                        Debug.Log($"Transição de cena: {scene} -> {targetScene}");

                        // Salva a posição do jogador antes de trocar de cena
                        SavePlayerPosition();
                        SceneManager.LoadScene(targetScene);
                        break;
                    }
                }
            }
        }
    }

    public void LoseLife(int amount)
    {
        lifes -= amount;
        Debug.Log("Vidas restantes: " + lifes);
        UpdateHeartsUI();

        if (lifes > 0)
            SavePlayerPosition();
        else
            RestartGame();
    }

    private void UpdateHeartsUI()
    {
        // Proteção contra chamar antes de apontar heartsUI
        if (heartsUI == null) return;

        for (int i = 0; i < heartsUI.Length; i++)
        {
            if (heartsUI[i] != null)
                heartsUI[i].gameObject.SetActive(i < lifes);
        }
    }

    private void UpdateTimeUI()
    {
        if (_timeTextUI == null)
            return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Cena1")
        {
            _timeTextUI.gameObject.SetActive(false);
        }
        else
        {
            _timeTextUI.gameObject.SetActive(true);
            int secs = Mathf.FloorToInt(currentTime);
            _timeTextUI.text = secs + "s";
        }
    }

    // public void GameOver()
    // {
    //     lifes = 3; // Reset lifes when game is over
    //     PlayerScore1 = 0; // Reset score when game is over
    //     savedPositions.Clear(); // Limpa todas as posições salvas
    //     SceneManager.LoadScene("GameOver");
    // }

    public void RestartGame()
    {
        lifes = maxLifes;            // reset das vidas
        currentTime = gameTime;      // reset do tempo

        ResetRunData();              // limpa openedChests, etc.
        previousScene = null;        // MUITO IMPORTANTE: impede a transição automática
        savedPositions.Clear();      // opcional: limpa todas as posições salvas

        SceneManager.LoadScene("Cena2"); // reinicia o jogo
    }


    private bool IsSceneAfter(string sceneName) => sceneOrder.IndexOf(SceneManager.GetActiveScene().name) > sceneOrder.IndexOf(sceneName);

    public void ReduceTime(float penalty)
    {
        currentTime -= penalty; // Subtrai o tempo de penalidade
        if (currentTime < 0)
        {
            currentTime = 0; // Garante que o tempo não fique negativo
            RestartGame(); // Reinicia o jogo se o tempo acabar
        }
    }

    public void SetInteracting(bool interacting)
    {
        isInteracting = interacting;
        Debug.Log($"isInteracting: {isInteracting}");
    }

    // Chame isso quando o jogo iniciar uma nova run
    public void ResetRunData()
    {
        openedChests.Clear(); // Limpa os baús abertos
        collectedSpawnIDs.Clear(); // Limpa os IDs dos spawns coletados
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        Debug.Log($"Moedas no GameManager: {coinCount}");
    }

    private void UpdateCoinsUI()
    {
        if (_coinTextUI != null)
            _coinTextUI.text = coinCount.ToString();
    }

    // Adiciona um item ao inventário. Retorna `true` se coube.
    public bool AddInventoryItem(ItemType item)
    {
        // --- evita duplicar Brasas de Bami ---
        if (item == ItemType.BamiEmber)
        {
            // se já tiver uma brasa no inventário, não adiciona outra
            for (int j = 0; j < inventory.Length; j++)
                if (inventory[j] == ItemType.BamiEmber)
                    return false;
        }

        // lógica normal de alocar um slot vazio
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == ItemType.None)
            {
                inventory[i] = item;
                RefreshInventorySlot(i);
                return true;
            }
        }
        return false; // inventário cheio
    }

    // Remove o item do slot selecionado.
    public void RemoveInventoryItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Length) return;
        inventory[slotIndex] = ItemType.None;
        RefreshInventorySlot(slotIndex);
    }

    // Atualiza visualmente um slot na UI.
    private void RefreshInventorySlot(int i)
    {
        Image slotImg = inventorySlotImages[i];
        if (slotImg == null) return;

        if (inventory[i] == ItemType.None)
        {
            // sem item → volta pro sprite vazio
            slotImg.sprite = emptySlotSprite;
        }
        else
        {
            // com item → usa o sprite específico do vetor itemIcons
            int idx = (int)inventory[i];
            if (idx >= 0 && idx < itemIcons.Length && itemIcons[idx] != null)
                slotImg.sprite = itemIcons[idx];
            else
                slotImg.sprite = emptySlotSprite; // fallback caso falte icone
        }
    }


    // Recupera vidas, sem ultrapassar o máximo.
    public void AddLife(int amount)
    {
        lifes = Mathf.Clamp(lifes + amount, 0, maxLifes);
        UpdateHeartsUI();
    }

    private IEnumerator StopTimeCoroutine()
    {
        IsTimeStopped = true;
        // roda aguardando o tempo real, sem mexer no timeScale
        yield return new WaitForSecondsRealtime(brokenWatchDuration);
        IsTimeStopped = false;
    }

    private IEnumerator CloakNyxRoutine()
    {
        IsInvisible = true;

        // Ajusta todos os SpriteRenderers do jogador para alpha reduzido
        var rends = thePlayer.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in rends)
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0.3f);

        // aguarda o término do efeito
        yield return new WaitForSeconds(cloakDuration);

        // volta à visibilidade normal
        foreach (var r in rends)
            r.color = new Color(r.color.r, r.color.g, r.color.b, 1f);

        IsInvisible = false;
    }

    // chamado pelo BamiEmberCollectible:
    public void GrantFlamePower(GameObject flameBallPrefab)
    {
        // guarda o prefab e habilita o uso
        _flameBallPrefab = flameBallPrefab;
        _hasFlamePower = true;

        // incrementa o contador de brasas
        bamiEmberCount++;
        Debug.Log($"Brasa de Bami adquirida! Total de Brasas: {bamiEmberCount}");

        // atualiza o ícone de poder (se houver)
        if (_powerIconUI != null)
            _powerIconUI.sprite = flameBallIcon;
    }

    public int GetFlameBallDamage()
    {
        return baseFlameDamage + damagePerEmber * bamiEmberCount;
    }

    public Sprite GetItemIcon(ItemType itemType)
    {
        int idx = (int)itemType;
        if (idx >= 0 && idx < itemIcons.Length && itemIcons[idx] != null)
            return itemIcons[idx];
        return emptySlotSprite;
    }

    private void SetCronosPowerSlot(int slotIdx, ItemType item)
    {
        if (_cronosSlotImages == null) return;
        if (slotIdx < 0 || slotIdx >= _cronosSlotImages.Length) return;
        _cronosSlotImages[slotIdx].sprite = GetItemIcon(item);
    }

    public void RegisterCollectedSpawn(string spawnID)
    {
        if (!collectedSpawnIDs.Contains(spawnID))
            collectedSpawnIDs.Add(spawnID);
    }

    // Usa (consome) o item naquele slot e aplica o efeito.
    public void UseInventoryItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Length) return;

        ItemType item = inventory[slotIndex];
        switch (item)
        {
            case ItemType.HealthPotion: // pocao de vida
                AddLife(2);   // recupera 2 corações
                break;
            // case ItemType.Key: … etc
            case ItemType.Hourglass: // ampulheta
            // adiciona tempo 
                currentTime += hourglassBonusTime;
            // limitar a um máximo:
                currentTime = Mathf.Min(currentTime, gameTime);
                Debug.Log($"Ampulheta usada! +{hourglassBonusTime}s");
                break;
            case ItemType.BrokenWatch: // relógio quebrado
                StartCoroutine(StopTimeCoroutine());
                break;
            case ItemType.CloakNyx: // véu de Nyx
                // inicia a invisibilidade sem bloquear o jogador
                StartCoroutine(CloakNyxRoutine());
                break;
            case ItemType.BamiEmber: // brasa de Bami
                GrantFlamePower(_flameBallPrefab);
                return; // não remove do inventário
            // case ItemType.OracleEye:
            //     SetCronosPowerSlot(0, ItemType.OracleEye);
            //     RemoveInventoryItem(slotIndex);
            //     return;

            // case ItemType.CronosMark:   // assim que definir essa enum
            //     Debug.Log("Usou Marca de Cronos!");
            //     SetCronosPowerSlot(1, ItemType.CronosMark);
            //     RemoveInventoryItem(slotIndex);
            //     return;
        }

        // remove do inventário
        RemoveInventoryItem(slotIndex);
    }

    
}

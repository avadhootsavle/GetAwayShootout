using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Gamemanager: MonoBehaviour
{
    private static GameState state;

    public int numPlayers = 4;
    public int numHumanPlayers = 1;

    public GameObject playerObj;
    public CameraManager cameraManager;
    public ScoreboardUI scoreboardUI;

    private Player[] players;
    private Camera[] cams;
    private PlayerInputManager inputManager;
    private WaypointManager waypointManager;
    private MapManager mapManager;
    private Finish finish;
    
    private bool roundOver;
    private CharacterSelectUI charSelectUI;
    private VictoryScreenUI victoryUI;
    private ArcadeSessionUI sessionUI;
    private CharacterDeathAudio deathAudio;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { state = null; }

    public void ReturnToLobby() { Time.timeScale = 1; OnChangeCharacters(); }

    private void Awake()
    {
        BackgroundMusic.EnsurePlaying();
        if (state == null)
            state = new GameState(numPlayers);

        inputManager = GetComponent<PlayerInputManager>();
        mapManager = GetComponent<MapManager>();

        scoreboardUI.Init(state.GetActiveScores());
        state.OnScoreChanged += scoreboardUI.UpdateScores;

        // Ensure CharacterSelectUI and VictoryScreenUI exist on Canvas
        Canvas canvas = scoreboardUI.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            sessionUI = canvas.gameObject.AddComponent<ArcadeSessionUI>();
            sessionUI.Init(this, canvas);
            charSelectUI = canvas.GetComponent<CharacterSelectUI>();
            if (charSelectUI == null)
                charSelectUI = canvas.gameObject.AddComponent<CharacterSelectUI>();

            victoryUI = canvas.GetComponent<VictoryScreenUI>();
            if (victoryUI == null)
                victoryUI = canvas.gameObject.AddComponent<VictoryScreenUI>();

            victoryUI.OnRematchRequested += OnRematch;
            victoryUI.OnChangeCharactersRequested += OnChangeCharacters;
        }

        if (!state.matchStarted)
        {
            // Show character select screen
            charSelectUI.Init(state);
            charSelectUI.OnMatchStartRequested += OnMatchStartedFromUI;
            charSelectUI.Show();
            scoreboardUI.gameObject.SetActive(false);
        }
        else
        {
            if (charSelectUI != null)
                charSelectUI.Hide();
            StartGameSession();
        }
    }

    private void OnDestroy()
    {
        state.OnScoreChanged -= scoreboardUI.UpdateScores;
        if (charSelectUI != null) charSelectUI.OnMatchStartRequested -= OnMatchStartedFromUI;
        if (victoryUI != null)
        {
            victoryUI.OnRematchRequested -= OnRematch;
            victoryUI.OnChangeCharactersRequested -= OnChangeCharacters;
        }
    }

    private void OnMatchStartedFromUI()
    {
        state.matchStarted = true;
        state.numHumanPlayers = charSelectUI.GetSelectedMode();
        state.enableBots = charSelectUI.GetBotsEnabled();
        state.p1Character = charSelectUI.GetP1Character();
        state.p2Character = charSelectUI.GetP2Character();
        state.ResetPlayerOrder();

        scoreboardUI.Init(state.GetActiveScores());

        StartGameSession();
    }

    private void StartGameSession()
    {
        scoreboardUI.gameObject.SetActive(true);
        numHumanPlayers = state.numHumanPlayers;
        deathAudio=gameObject.AddComponent<CharacterDeathAudio>();
        deathAudio.Configure(numHumanPlayers);
        int activeCount = state.TotalPlayers;
        players = new Player[activeCount];

        CreateInput();
        CreateMap();
        CreatePlayers();
        scoreboardUI.BindPlayers(players);
        CreateCameras();
        sessionUI.BeginRound();
    }

    void CreateInput()
    {
        for (int i = 0; i < numHumanPlayers; i++)
        {
            if (i == 0)
                inputManager.JoinPlayer(playerIndex: i, controlScheme: "KeyboardLeft", pairWithDevices: Keyboard.current);
            else if (i == 1)
                inputManager.JoinPlayer(playerIndex: i, controlScheme: "KeyboardRight", pairWithDevices: Keyboard.current);
            else
                inputManager.JoinPlayer(playerIndex: i, controlScheme: "Gamepad");
        }   
    }

    void CreateMap()
    {
        mapManager.CreateMap(state);

        waypointManager = FindAnyObjectByType<WaypointManager>();
        if (waypointManager == null)
            throw new System.Exception("Map does not contain Waypoint Manager! Add waypoint manager to map for navigation and spawn points.");
        finish = FindAnyObjectByType<Finish>();
        if (finish == null)
            Debug.LogError("Map does not contain finish! Add finish object for players to complete the map.");
        else
        {
            finish.OnPlayersFinished += RoundOver;
            finish.OnPlayerEntered += OnPlayerFinished;
        }
    }

    void CreatePlayers()
    {
        int activeCount = state.TotalPlayers;
        for (int i = 0; i < activeCount; i++)
        {
            int playerID = state.playerOrder[i];

            GameObject player = Instantiate(playerObj);
            Player pComponent = player.GetComponent<Player>();
            pComponent.SetPlayerID(playerID);


            players[playerID] = pComponent;

            // Character skin assignment
            int charIdx = 0;
            if (playerID == 0)
                charIdx = state.p1Character;
            else if (playerID == 1 && numHumanPlayers >= 2)
                charIdx = state.p2Character;
            else if (playerID == 1)
                charIdx = (state.p1Character == 1) ? 2 : 1;
            else if (playerID == 2)
                charIdx = (state.p1Character == 2 || state.p2Character == 2) ? 0 : 2;
            else
                charIdx = 3;

            pComponent.SetCharacter(charIdx);

            players[playerID].onDeathEvent += OnPlayerDeath;
            waypointManager.AddNav(player.transform);

            Vector2 spawnPos = waypointManager.GetNavCurWaypoint(player.transform).transform.position;
            spawnPos += Vector2.right * 0.75f * i;
            player.GetComponent<PlayerBody>().SetBody(spawnPos, 0f, 0f);

            if (playerID < numHumanPlayers)
                player.AddComponent<PlayerHumanController>();
            else
                player.AddComponent<PlayerComputerController>();

        }
    }

    void CreateCameras()
    {
        cams = cameraManager.Initalize(numHumanPlayers);
        GetComponent<ParallaxManager>().cameras = cams;

        for (int i = 0; i < cams.Length; i++)
        {
            Health h = players[i].GetComponent<Health>();
            cams[i].GetComponentInChildren<HealthUI>().Init(h);

            PlayerPowerUp pu = players[i].GetComponent<PlayerPowerUp>();
            cams[i].GetComponentInChildren<PowerUpUI>().Init(pu);
        }
    }

    public Player GetPlayer(int playerID)
    {
        return players[playerID];
    }

    void OnPlayerDeath(Player player)
    {
        if(!roundOver) deathAudio.PlayDeath(player);
        StartCoroutine(RespawnPlayerLoop(player));
    }

    void OnPlayerFinished(Player player, bool first)
    {
        state.PlayerFinished(player.GetPlayerID());
    }

    void RoundOver()
    {
        if (roundOver) return;
        roundOver = true;
        sessionUI.FinishRound();
        state.RoundOver(GetPlayerOrder());

        if (players != null)
        {
            foreach (Player player in players)
            {
                if (player != null)
                    player.Deactivate();
            }
        }

        if (cams != null && finish != null)
        {
            foreach(Camera cam in cams)
            {
                if (cam != null)
                    cam.gameObject.GetComponent<CameraFollow>().SetTarget(finish.transform, false);
            }
        }

        if (state.GetWinner() >= 0)
            StartCoroutine(GameOverLoop());
        else
            StartCoroutine(RoundOverLoop());
    }

    int[] GetPlayerOrder()
    {
        Transform[] navs = waypointManager.GetNavDistanceOrder();
        int[] ids = new int[navs.Length];
        for (int i = 0; i < navs.Length; i++)
            ids[i] = navs[i].GetComponent<Player>().GetPlayerID();
        
        return ids;
    }

    IEnumerator GameOverLoop()
    {
        yield return new WaitForSeconds(1.2f);
        int winnerId = state.GetWinner();

        int winnerChar = 0;
        if (winnerId == 0) winnerChar = state.p1Character;
        else if (winnerId == 1 && numHumanPlayers >= 2) winnerChar = state.p2Character;
        else if (winnerId == 1) winnerChar = (state.p1Character == 1) ? 2 : 1;
        else if (winnerId == 2) winnerChar = (state.p1Character == 2 || state.p2Character == 2) ? 0 : 2;
        else winnerChar = 3;

        if (victoryUI != null)
        {
            victoryUI.ShowVictory(winnerId, winnerChar, state.GetActiveScores());
        }
    }

    private void OnRematch()
    {
        state.ResetMatch();
        state.matchStarted = true;
        SceneManager.LoadScene(0);
    }

    private void OnChangeCharacters()
    {
        state.ResetMatch();
        state.matchStarted = false;
        SceneManager.LoadScene(0);
    }

    IEnumerator RoundOverLoop()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(0);
    }

    IEnumerator RespawnPlayerLoop(Player player)
    {
        yield return new WaitForSeconds(1.5f);

        if (roundOver || player == null)
            yield break;

        player.Respawn();
        Vector2 spawnPos = waypointManager.GetNavSpawnWaypoint(player.transform).transform.position;
        player.GetComponent<PlayerBody>().SetBody(spawnPos, 0f, 0f);
    }

    private void OnDisable()
    {
        if (players != null)
        {
            for(int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                    players[i].onDeathEvent -= OnPlayerDeath;
            }
        }
    }
}

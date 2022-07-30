using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    private void Initialize()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Terminate()
    {
        if (this == Instance)
        {
            instance = null;
        }
    }
    #endregion

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject projectilePrefab;
    public MenuUIManager MenuUIManager;
    public InGameUIManager InGameUIManager;

    public int MaxRoundCount = 5;
    public int CurrentRound;
    

    public ProjectileController Projectile;

    private List<RoomInfo> roomList;

    public Dictionary<int, PlayerController> Players = new Dictionary<int, PlayerController>();
    private int playerCount;

    public Color32[] PlayerColors = new Color32[]
    {
        new Color32(203,47,44, 255), // red
        new Color32(27,150,186,255), //blue
        new Color32(241,190,67,255), //yellow
        new Color32(75,140,45,255) //green
    };

    private void Awake()
    {
        Initialize();
        MenuUIManager = FindObjectOfType<MenuUIManager>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        if (_scene == SceneManager.GetSceneByBuildIndex(1))
        {
            InGameUIManager = FindObjectOfType<InGameUIManager>();
            InstantiateLocalPlayer();
            InstantiateLocalProjectile();
        }
        if (_scene == SceneManager.GetSceneByBuildIndex(0))
        {
            MenuUIManager = FindObjectOfType<MenuUIManager>();
        }
    }
    private void OnDestroy()
    {
        Terminate();
    }

    #region PhotonCallbacks
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
    }
    public override void OnRoomListUpdate(List<RoomInfo> _list)
    {

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            roomList = _list;
            MenuUIManager.CreateNewRoomButtons(roomList);
        }

        base.OnRoomListUpdate(roomList);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            MenuUIManager.OnJoined();
            MenuUIManager.UpdatePlayerPanels();

            if (PhotonNetwork.IsMasterClient)
                MenuUIManager.ShowHostStartButton();
            else
                MenuUIManager.ShowHostStartButton(false);
        }

        base.OnJoinedRoom();
    }
    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            MenuUIManager.ShowHostStartButton(false);
            MenuUIManager.OnLeft();
        }
        base.OnLeftRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            MenuUIManager.UpdatePlayerPanels();
        }

        base.OnPlayerEnteredRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            MenuUIManager.UpdatePlayerPanels();

            if (PhotonNetwork.IsMasterClient)
                MenuUIManager.ShowHostStartButton();
            else
                MenuUIManager.ShowHostStartButton(false);
        }

        base.OnPlayerLeftRoom(otherPlayer);
    }

    #endregion

    #region InGame Functions
    private void InstantiateLocalPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (PlayerController.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                Vector2 rndPos = Random.insideUnitCircle.normalized * 6;
                PhotonNetwork.Instantiate(this.playerPrefab.name, rndPos, Quaternion.identity);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }
    private void InstantiateLocalProjectile()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("Missing projectilePrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                PhotonNetwork.Instantiate(projectilePrefab.name, Vector2.zero, Quaternion.identity);
                photonView.RPC("GetProjectilePrefab", RpcTarget.AllBufferedViaServer);
            }
        }
    }
    [PunRPC]
    private void GetProjectilePrefab()
    {
        Projectile = FindObjectOfType<ProjectileController>();
    }

    public void RegisterPlayer(int _viewID, PlayerController _controller)
    {
        Players.Add(_viewID, _controller);
        playerCount++;
        Debug.Log($"Added {_viewID} to the List");
    }
    public void UnRegisterPlayer(int _viewID)
    {
        if (Players.ContainsKey(_viewID))
        {
            Players.Remove(_viewID);
            playerCount--;
        }
    }

    public void PlayerDied(int _viewID)
    {
        UpdateAllPlayers(_viewID);

        if (Players.Count > 0)
        {
            Debug.LogError(_viewID + " died!");
            playerCount--;
        }

        CalculateWin();
    }

    public void RepositionPlayers()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players.ElementAt(i).Value.Reposition();
        }
    }

    private void UpdateAllPlayers(int _viewID)
    {
        foreach (var player in Players)
        {
            if (player.Key == _viewID)
            {
                player.Value.OnDie();
            }
        }
    }
    private void CalculateWin()
    {
        if (playerCount == 1)
        {
            if (CurrentRound < MaxRoundCount)
                InGameUIManager.EndOfRound();
            else
                InGameUIManager.EndOfMatch();
        }
    }
    public PlayerController GetWinner()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (!Players.ElementAt(i).Value.IsDead)
            {
                Players.ElementAt(i).Value.Wins++;
                return Players.ElementAt(i).Value;
            }
        }
        return null;
    }

    public void StartRound()
    {
        playerCount = Players.Count;
        for (int i = 0; i < Players.Count; i++)
        {
            Players.ElementAt(i).Value.OnRespawn();
        }
        Projectile.transform.position = Vector2.zero;
        Projectile.StopFlying();
        CurrentRound++;
    }
    #endregion
}
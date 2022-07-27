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

    private List<RoomInfo> roomList;

    public Dictionary<int, PlayerController> Players = new Dictionary<int, PlayerController>();

    private void Awake()
    {
        Initialize();
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
            InstantiateLocalPlayer();
            if (PhotonNetwork.IsMasterClient)
                InstantiateLocalProjectile();
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
        roomList = _list;
        UIManager.Instance.CreateNewRoomButtons(roomList);

        base.OnRoomListUpdate(roomList);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        UIManager.Instance.OnJoined();
        UIManager.Instance.UpdatePlayerPanels();

        if (PhotonNetwork.IsMasterClient)
            UIManager.Instance.ShowHostStartButton();
        else
            UIManager.Instance.ShowHostStartButton(false);

        base.OnJoinedRoom();
    }
    public override void OnLeftRoom()
    {
        UIManager.Instance.ShowHostStartButton(false);
        UIManager.Instance.OnLeft();
        base.OnLeftRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UIManager.Instance.UpdatePlayerPanels();

        base.OnPlayerEnteredRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UIManager.Instance.UpdatePlayerPanels();

        if (PhotonNetwork.IsMasterClient)
            UIManager.Instance.ShowHostStartButton();
        else
            UIManager.Instance.ShowHostStartButton(false);

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
                GameObject go = PhotonNetwork.Instantiate(this.playerPrefab.name, rndPos, Quaternion.identity);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }
    private void InstantiateLocalProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Missing projectilePrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            PhotonNetwork.Instantiate(projectilePrefab.name, Vector2.zero, Quaternion.identity);
        }
    }

    public void RegisterPlayer(int _viewID, PlayerController _controller)
    {
        Players.Add(_viewID, _controller);
        Debug.Log($"Added {_viewID} to the List");
    }

    public void PlayerDied(int _viewID)
    {
        UpdateAllPlayers(_viewID);

        if (Players.Count > 0)
        {
            Debug.LogError(_viewID + " died!");
            Players.Remove(_viewID);
        }

        CalculateWin();
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
        if (Players.Count == 1)
        {
            Debug.LogError($"{Players.ElementAt(0).Key} won!");

            /*
             * Show GameResult UI
             * 'PLAYER X WON' etc
             */
        }
    }
    #endregion
}
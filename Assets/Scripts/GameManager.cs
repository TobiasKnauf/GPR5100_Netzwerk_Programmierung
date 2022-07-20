using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

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
    [SerializeField] private UIManager UIManager;

    private List<RoomInfo> roomList;

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
            InstantiateLocalPlayer();
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
        UIManager.CreateNewRoomButtons(roomList);

        base.OnRoomListUpdate(roomList);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        UIManager.OnJoined();
        UIManager.UpdatePlayerPanels();

        if (PhotonNetwork.IsMasterClient)
            UIManager.ShowHostStartButton();
        else
            UIManager.ShowHostStartButton(false);

        base.OnJoinedRoom();
    }
    public override void OnLeftRoom()
    {
        UIManager.ShowHostStartButton(false);
        UIManager.OnLeft();
        base.OnLeftRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UIManager.UpdatePlayerPanels();

        base.OnPlayerEnteredRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UIManager.UpdatePlayerPanels();

        if (PhotonNetwork.IsMasterClient)
            UIManager.ShowHostStartButton();
        else
            UIManager.ShowHostStartButton(false);

        base.OnPlayerLeftRoom(otherPlayer);
    }

    #endregion

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
                PhotonNetwork.Instantiate(this.playerPrefab.name, this.transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }
}
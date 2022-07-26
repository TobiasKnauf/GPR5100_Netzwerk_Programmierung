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

    private List<RoomInfo> roomList;

    public List<PhotonView> Players;

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
                GameObject go = PhotonNetwork.Instantiate(this.playerPrefab.name, this.transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

    public void RegisterPlayer(int _viewID)
    {
        Players.Add(PhotonNetwork.GetPhotonView(_viewID));
        Debug.Log($"Added {_viewID} to the List");
    }

    public void PlayerDied(int _viewID)
    {
        if (Players.Count > 0)
        {
            Debug.LogError(_viewID + " died!");
            if (PlayerController.LocalPlayerInstance.photonView.ViewID == _viewID)
            {
                PlayerController.LocalPlayerInstance.Died();
            }
            Players.Remove(PhotonNetwork.GetPhotonView(_viewID));
        }

        CalculateWin();
    }
    private void CalculateWin()
    {
        if(Players.Count == 1)
        {
            Debug.LogError($"{Players[0]} won!");    

            /*
             * Show GameResult UI
             * 'PLAYER X WON' etc
             */
        }
    }
    #endregion
}
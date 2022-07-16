using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerNetworkManager player;

    [Header("Panel/Screens")]
    [SerializeField] private Canvas m_StartPanel;
    [SerializeField] private Canvas m_RoomListPanel;
    [SerializeField] private Canvas m_RoomPanel;
    [SerializeField] private Canvas m_OptionsPanel;

    [Header("StartPanel Objects")]
    [SerializeField] private Image m_ControlsImage;
    [SerializeField] private Image m_GameplayImage;
    [SerializeField] private TMP_InputField m_UserNameInput;

    [Header("RoomCreation Objects")]
    [SerializeField] private TMP_InputField m_RoomNameInput;
    [SerializeField] private TMP_Dropdown m_RoomPlayerCount;
    [SerializeField] private TMP_Dropdown m_RoomMapIndex;

    [Header("RoomList Objects")]
    [SerializeField] public GameObject m_RoomListEntryPrefab;
    [SerializeField] public Transform m_RoomListContent;

    [Header("RoomPanel Objects")]
    [SerializeField] private TMP_Text m_RoomTitleText;
    [SerializeField] private Transform m_ConnectedPlayersContent;
    [SerializeField] private GameObject m_ListedPlayerPrefab;
    [SerializeField] private GameObject m_HostStartButton;

    #region UI Updates
    private void CloseAllPanels()
    {
        m_StartPanel.enabled = false;
        m_RoomListPanel.enabled = false;
        m_RoomPanel.enabled = false;
        m_OptionsPanel.enabled = false;
    }
    /// <summary>
    /// Disables all panels and shows the give one
    /// </summary>
    /// <param name="_panel">Panel to show</param>
    public void ShowPanel(Canvas _panel)
    {
        CloseAllPanels();
        _panel.enabled = true;
    }
    /// <summary>
    /// Creates a new List of Room Buttons
    /// </summary>
    /// <param name="_roomList">All rooms in the default Lobby</param>
    public void CreateNewRoomButtons(List<RoomInfo> _roomList)
    {
        //Clear Room list
        foreach (Transform t in m_RoomListContent)
            Destroy(t.gameObject);

        for (int i = 0; i < _roomList.Count; i++)
        {
            if (_roomList[i].RemovedFromList) continue;

            GameObject newRoomListEntry = Instantiate(m_RoomListEntryPrefab, m_RoomListContent);
            newRoomListEntry.transform.Find("Txt_RoomName").GetComponent<TMP_Text>().text = _roomList[i].Name; //Set server name to button
            newRoomListEntry.transform.Find("Txt_Players").GetComponent<TMP_Text>().text = $"{_roomList[i].PlayerCount}/{_roomList[i].MaxPlayers}"; //Set current player and max players to button
            newRoomListEntry.GetComponent<Button>().onClick.AddListener(delegate { OnClick_JoinRoom(newRoomListEntry); }); //Assigns new Function to this button
        }
    }
    /// <summary>
    /// Creates a new List of Player in the current Room
    /// </summary>
    public void UpdatePlayerPanels()
    {
        //Clear Player Panels
        foreach (Transform t in m_ConnectedPlayersContent)
            Destroy(t.gameObject);

        Color[] colors =
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
        };

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject newPlayer = Instantiate(m_ListedPlayerPrefab, m_ConnectedPlayersContent);
            newPlayer.GetComponentInChildren<TMP_Text>().text = PhotonNetwork.PlayerList[i].NickName; //Show player name
            newPlayer.GetComponent<Image>().color = colors[i]; //Set player color
        }
    }
    public void ShowHostStartButton(bool _visibility = true)
    {
        m_HostStartButton.SetActive(_visibility);
    }
    #endregion

    #region Callback Methods
    /// <summary>
    /// Called when the user joins a room
    /// </summary>
    public void OnJoined()
    {
        ShowPanel(m_RoomPanel);
        m_RoomTitleText.text = PhotonNetwork.CurrentRoom.Name;
    }
    /// <summary>
    /// Called when the user leaves a room
    /// </summary>
    public void OnLeft()
    {
        ShowPanel(m_RoomListPanel);
    }
    /// <summary>
    /// Called when the MasterClient starts the match
    /// </summary>
    private void OnStart()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;

        //PhotonNetwork.LoadLevel(#GAMESCENE);
    }
    #endregion

    #region Button Click Events
    public void OnClick_CreateRoom()
    {
        if (!string.IsNullOrEmpty(m_UserNameInput.text))
            PhotonNetwork.NickName = m_UserNameInput.text;
        else
            PhotonNetwork.NickName = "Guest";

        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = (byte)(m_RoomPlayerCount.value + 2),
            IsVisible = true, //needs to be true so the roomlist will be updated
            IsOpen = true,
        };
        PhotonNetwork.CreateRoom(m_RoomNameInput.text, options);
    }
    public void OnClick_JoinRoom(GameObject _button)
    {
        if (!string.IsNullOrEmpty(m_UserNameInput.text))
            PhotonNetwork.NickName = m_UserNameInput.text;
        else
            PhotonNetwork.NickName = "Guest";

        string roomName = _button.transform.Find("Txt_RoomName").GetComponent<TMP_Text>().text;
        PhotonNetwork.JoinRoom(roomName);
    }
    public void OnClick_LeaveRoom()
    {
        if (PhotonNetwork.InRoom == false) return;

        PhotonNetwork.LeaveRoom(false);
    }
    public void OnClick_StartRound()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        OnStart();
    }
    #endregion
}

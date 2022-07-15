using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public List<RoomInfo> Rooms = new List<RoomInfo>();

    [Header("Panel/Screens")]
    [SerializeField] private GameObject m_StartPanel;
    [SerializeField] private GameObject m_RoomListPanel;
    [SerializeField] private GameObject m_RoomPanel;
    [SerializeField] private GameObject m_OptionsPanel;

    [Header("StartPanel Objects")]
    [SerializeField] private Image m_ControlsImage;
    [SerializeField] private Image m_GameplayImage;

    [Header("RoomCreation Objects")]
    [SerializeField] private TMP_InputField m_RoomNameInput;
    [SerializeField] private TMP_Dropdown m_RoomPlayerCount;
    [SerializeField] private TMP_Dropdown m_RoomMapIndex;

    [Header("RoomList Objects")]
    [SerializeField] private GameObject m_NewRoomParent;
    [SerializeField] private RoomListEntry m_RoomListEntryPrefab;

    [Header("User Information")]
    public string UserRoom;

    private void Update()
    {
        /*
            update room list
            update room list in ui
        */
        for (int i = 0; i < Rooms.Count; i++)
        {
            //Rooms[i].
        }


        Debug.Log($"Amount of rooms: {Rooms.Count}");
        if (Rooms.Count>0)
            Debug.Log($"Room 1 Info Name: {Rooms[0].LobbyName}, Players: {Rooms[0].CurrentPlayerCount}/{Rooms[0].MaxPlayerCount}, Map: {Rooms[0].MapIndex}");
    }

    public void ShowPanel(GameObject _panel)
    {
        m_StartPanel.SetActive(false);
        m_RoomListPanel.SetActive(false);
        m_RoomPanel.SetActive(false);
        m_OptionsPanel.SetActive(false);

        _panel.SetActive(true);
    }
    public void CreateRoom()
    {
        RoomListEntry roomPanel = Instantiate(m_RoomListEntryPrefab, m_NewRoomParent.transform);
        //roomPanel.Init(m_RoomNameInput.text, m_RoomPlayerCount.value + 2, m_RoomMapIndex.value);
        Rooms.Add(roomPanel.RoomInfo);
    }
}

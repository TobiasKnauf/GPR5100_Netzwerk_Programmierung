using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayerPrefab;

    void Start()
    {
        PhotonNetwork.Instantiate(m_PlayerPrefab.name, this.transform.position, Quaternion.identity);
    }
}

using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    PhotonView photonView;
    GameObject controller;

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        StartCoroutine(DelayedRespawn());
    }

    private IEnumerator DelayedRespawn()
    {
        PhotonNetwork.SendAllOutgoingCommands();
        yield return new WaitForSeconds(0.2f);
        CreateController();
    }

    void Awake() => photonView = GetComponent<PhotonView>();

    void Start()
    {
        if (photonView.IsMine)
            CreateController();
    }

    void CreateController()
    {
        Debug.Log("@PlayerManager - Create Controller");
        var spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"),
            spawnPoint.position, spawnPoint.rotation, 0, new object[] { photonView.ViewID });
    }
}
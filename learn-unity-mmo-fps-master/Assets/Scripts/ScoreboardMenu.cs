using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Simple scoreboard menu — displays player nicknames, kills and deaths
/// Opens/closes on Tab key press
/// </summary>
public class ScoreboardMenu : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private Transform scoreboardContent;
    [SerializeField] private GameObject playerScoreRowPrefab;

    [Header("Key Settings")]
    [SerializeField] KeyCode toggleKey = KeyCode.Tab;

    private Dictionary<string, ScoreRow> scoreRows = new Dictionary<string, ScoreRow>();
    private VerticalLayoutGroup layoutGroup;

    void Awake()
    {
        scoreboardPanel.SetActive(false);

        layoutGroup = scoreboardContent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = scoreboardContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleScoreboard();
        }
    }

    private void ToggleScoreboard()
    {
        bool isActive = !scoreboardPanel.activeSelf;
        scoreboardPanel.SetActive(isActive);

        if (isActive)
            UpdateScoreboard();
    }

    /// <summary>
    /// Обновляет значения в существующих строках, создаёт новые при необходимости.
    /// </summary>
    private void UpdateScoreboard()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string nickname = player.NickName;

            if (!scoreRows.ContainsKey(nickname))
            {
                // Создаём новую строку
                GameObject row = Instantiate(playerScoreRowPrefab, scoreboardContent);
                ScoreRow rowScript = row.GetComponent<ScoreRow>();
                if (rowScript != null)
                {
                    rowScript.Setup(player);
                    scoreRows[nickname] = rowScript;
                }
            }
            else
            {
                // Обновляем существующую
                scoreRows[nickname].Setup(player);
            }
        }

        // Удаляем строки для игроков, которых больше нет
        List<string> currentNicknames = new List<string>();
        foreach (Player p in PhotonNetwork.PlayerList)
            currentNicknames.Add(p.NickName);

        List<string> toRemove = new List<string>();
        foreach (var kvp in scoreRows)
        {
            if (!currentNicknames.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }

        foreach (string key in toRemove)
        {
            Destroy(scoreRows[key].gameObject);
            scoreRows.Remove(key);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Создаём строку для нового игрока, если её ещё нет
        if (!scoreRows.ContainsKey(newPlayer.NickName))
        {
            GameObject row = Instantiate(playerScoreRowPrefab, scoreboardContent);
            ScoreRow rowScript = row.GetComponent<ScoreRow>();
            if (rowScript != null)
            {
                rowScript.Setup(newPlayer);
                scoreRows[newPlayer.NickName] = rowScript;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Удаляем строку ушедшего игрока
        if (scoreRows.ContainsKey(otherPlayer.NickName))
        {
            Destroy(scoreRows[otherPlayer.NickName].gameObject);
            scoreRows.Remove(otherPlayer.NickName);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Обновляем только если открыта таблица и изменились нужные свойства
        if (scoreboardPanel.activeSelf && (changedProps.ContainsKey("Kills") || changedProps.ContainsKey("Deaths")))
        {
            if (scoreRows.ContainsKey(targetPlayer.NickName))
                scoreRows[targetPlayer.NickName].Setup(targetPlayer);
        }
    }
}

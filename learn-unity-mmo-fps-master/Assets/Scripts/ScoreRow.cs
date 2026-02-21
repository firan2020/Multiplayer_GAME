using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Component for a single row in the scoreboard
/// </summary>
public class ScoreRow : MonoBehaviour
{
    [SerializeField] TMP_Text nicknameText;
    [SerializeField] TMP_Text killsText;
    [SerializeField] TMP_Text deathsText;

    /// <summary>
    /// Setup row with player data
    /// </summary>
    public void Setup(Player player)
    {
        if (nicknameText != null)
            nicknameText.text = player.NickName;
        else
            Debug.LogError("nicknameText не назначен в префабе ScoreRow!");

        int kills = player.CustomProperties.ContainsKey("Kills")
            ? (int)player.CustomProperties["Kills"]
            : 0;
        if (killsText != null)
            killsText.text = kills.ToString();
        else
            Debug.LogError("killsText не назначен в префабе ScoreRow!");

        int deaths = player.CustomProperties.ContainsKey("Deaths")
            ? (int)player.CustomProperties["Deaths"]
            : 0;
        if (deathsText != null)
            deathsText.text = deaths.ToString();
        else
            Debug.LogError("deathsText не назначен в префабе ScoreRow!");

        Debug.Log($"Setting up row for: {player.NickName}, Kills: {kills}, Deaths: {deaths}");
    }
}
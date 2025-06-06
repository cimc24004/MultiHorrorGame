using TMPro;
using UnityEngine;

public class LobbyID : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI m_lobbyIdText;

    public void LobbyIdSet()
    {
        //ロビー作成or入室時に記憶しておいたLobbyIDを設定
        m_lobbyIdText.text = SteamLobby.Instance.LobbyID.ToString();
        GUIUtility.systemCopyBuffer = m_lobbyIdText.text;
    }
}
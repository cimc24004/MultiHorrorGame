using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerClientBox : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI PlayerClientSteamUserNameTextUI;
    [SerializeField] private TextMeshProUGUI PlayerClientRoleNameTextUI;
    public void PlayerOnClientConnectedSetText(string _SteamUserName, string _Role)
    {
        PlayerClientSteamUserNameTextUI.SetText(_SteamUserName.ToString());
        PlayerClientRoleNameTextUI.SetText(_Role);
    }
}

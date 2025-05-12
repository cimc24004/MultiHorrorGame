using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerClientBox : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI PlayerClientIdTextUI;

    public void PlayerOnClientConnectedSetText(string _clientId){
        PlayerClientIdTextUI.SetText(_clientId.ToString());
    }
}

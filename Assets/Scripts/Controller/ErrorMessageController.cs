using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ErrorMessageController : MonoBehaviour
{
    [SerializeField] private GameObject ErrorCanvasObject;
    [SerializeField] private TextMeshProUGUI ErrorMessageTextUI;
    [SerializeField] private Button MainMenuButton;
    void Start()
    {
        MainMenuButton.onClick.AddListener(() =>
        {
            SessionController.Instance.ReturnToTitle();
            ErrorCanvasObject.gameObject.SetActive(false);
        });
    }
    public void ShowErrorUI(string _errorMessage)
    {
        //エラーメッセージポップアップを出す
        ErrorMessageTextUI.SetText(_errorMessage);
        ErrorCanvasObject.gameObject.SetActive(true);
    }
}

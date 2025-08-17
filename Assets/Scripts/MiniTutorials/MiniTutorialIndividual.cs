using UnityEngine;

public class MiniTutorialIndividual : MonoBehaviour
{
    private MiniTutorialController miniTutorialController;
    private MiniTutorialDatabases miniTutorialDatabases;

    [SerializeField] private int ShowId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        miniTutorialDatabases = MiniTutorialDatabases.instance;
    }

    // // Update is called once per frame
    // void Update()
    // {

    // }
    public void ShowMiniTutorialById()
    {
        if (miniTutorialController == null)
        {
            miniTutorialController = GameController.instance.miniTutorialController;
        }
        Debug.Log("miniTutorialController: " + miniTutorialController);
        miniTutorialController.ShowById(ShowId);
    }
}

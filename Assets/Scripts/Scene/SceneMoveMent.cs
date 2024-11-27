using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMoveMent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SceneMove(string _scenename)
    {
        SceneManager.LoadScene(_scenename);

    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }
}

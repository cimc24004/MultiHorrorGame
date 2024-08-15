using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StageRoutings : MonoBehaviour
{
    public List<GameObject> Routings;
    List<GameObject> gameObjs = new List<GameObject>();
    private int i = 0;
    private int k = 0;
    // Start is called before the first frame update
    void Start()
    {
        Routings = new List<GameObject>(GameObject.FindGameObjectsWithTag("Route"));
        //Routings.Sort();
        gameObjs = Routings.OrderBy(x => x.transform.GetSiblingIndex()).ToList();
        
        Routings = gameObjs;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("-------Start---------------");
        foreach(GameObject g in Routings) {
            Debug.Log("g: " + g);
        }
        Debug.Log("-------End---------------");
    }
}

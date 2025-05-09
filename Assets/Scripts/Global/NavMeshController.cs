using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
public class NavMeshController : MonoBehaviour
{
    private NavMeshSurface  _surface;
    // Start is called before the first frame update
    void Start()
    {
        _surface = this.gameObject.GetComponent<NavMeshSurface>();
        _surface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

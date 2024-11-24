using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Route : MonoBehaviour
{
    private TextDisplay text_display;
    // Start is called before the first frame update
    void Start()
    {
        text_display = this.gameObject.GetComponent<TextDisplay>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(this.transform.position, Vector3.up);
    }
}

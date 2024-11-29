using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangeIndividualDiscoveried : MonoBehaviour
{
    
    [SerializeField] private Material ChangeMaterial;
    
    private Renderer m_renderer;
    private Material DefaultMaterial;

    public MaterialChangeController material_change_controller;
    private void Start()
    {
        m_renderer = this.gameObject.GetComponent<Renderer>();
        DefaultMaterial = m_renderer.material;
    }

    private void Update()
    {
        if (material_change_controller.MaterialChange)
        {
            m_renderer.material = ChangeMaterial;
        }
        else
        {
            m_renderer.material = DefaultMaterial;
        }
    }
}

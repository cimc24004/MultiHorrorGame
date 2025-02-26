using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public class EnemyDeathController : SerializedMonoBehaviour
{
    public bool Death;
    private Animator m_Animator;
    [SerializeField]
    private Dictionary<SkinnedMeshRenderer, Material> mesh_renderer_with_death_material;
    private float DeathAlpha;
    [SerializeField]
    private AudioSource DeathAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        m_Animator = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Button]
    public void DeadStart()
    {
        if (Death) return;
        DeathAudioSource.Play();
        Death = true;
        m_Animator.SetBool("Death", Death);
    }
    public void DeathAnimationEnd()
    {
        foreach (KeyValuePair<SkinnedMeshRenderer, Material> skin_mesh_with_material in mesh_renderer_with_death_material)
        {
            SkinnedMeshRenderer skinned_mesh_renderer = skin_mesh_with_material.Key;
            Material DeathMaterial = skin_mesh_with_material.Value;
            skinned_mesh_renderer.material = DeathMaterial;
            Debug.Log("SkinnedMeshRenderer.Material=" + skinned_mesh_renderer.material.name);
            float startValue = skinned_mesh_renderer.material.GetFloat("_Threshold");
            DOTween.To(() => startValue, x => skinned_mesh_renderer.material.SetFloat("_Threshold", x), 5f, 3f).SetLink(this.gameObject).SetEase(Ease.Linear).OnComplete(() => {
                Destroy(this.gameObject);
            });
        }
    }

}

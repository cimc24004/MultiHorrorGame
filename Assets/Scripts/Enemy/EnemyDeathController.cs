using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.AI;
using Unity.Netcode;
public class EnemyDeathController : SerializedMonoBehaviour
{
    public bool Death;
    private Animator m_Animator;
    [SerializeField]
    private Dictionary<SkinnedMeshRenderer, Material> mesh_renderer_with_death_material;
    private float DeathAlpha;
    [SerializeField]
    public AudioSource DeathAudioSource;
    private NavMeshAgent nav_mesh_agent;
    private CharacterController character_controller;
    private FollowObjectByNav follow_object_by_nav;
    private DiscoveryPlayerController discoveryPlayerController;
    // Start is called before the first frame update
    void Start()
    {
        nav_mesh_agent = this.gameObject.GetComponent<NavMeshAgent>();
        character_controller = this.gameObject.GetComponent<CharacterController>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        follow_object_by_nav = this.gameObject.GetComponent<FollowObjectByNav>();
        discoveryPlayerController = this.gameObject.GetComponent<DiscoveryPlayerController>();
    }

    [Button]
    public void DeadStart()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (Death) return;
        Death = true;
        nav_mesh_agent.enabled = false;
        character_controller.Move(Vector3.zero);
        follow_object_by_nav.MovePermit = false;
        m_Animator.SetBool("Death", Death);
        for (int i = 0; i < discoveryPlayerController.DiscoveriedPlayers.Count; i++)
        {
            discoveryPlayerController.RemovePlayerOfDiscoveryPlayerLists(discoveryPlayerController.DiscoveriedPlayers[i]);
        }
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
            DOTween.To(() => startValue, x => skinned_mesh_renderer.material.SetFloat("_Threshold", x), 5f, 3f).SetLink(this.gameObject).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(this.gameObject);
            });
        }
    }

}

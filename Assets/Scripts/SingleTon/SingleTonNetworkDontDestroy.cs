using UnityEngine;
using Unity.Netcode;

/// <summary>
/// シーンをまたいで1個だけ生き残る NetworkSingleton（DontDestroyOnLoad版）
/// ・最初に生成された個体だけ生存
/// ・以降にロードされた重複は即Destroy（Spawn済みならDespawn）
/// ・Domain Reload無効でも OnDestroyでInstanceを解放
/// </summary>
[DefaultExecutionOrder(-10000)]
public abstract class SingleTonNetworkDontDestroy<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static T Instance { get; private set; }

    // 必要なら無効化。基本はtrueでOK
    [SerializeField] private bool useDontDestroyOnLoad = true;

    protected virtual void Awake()
    {
        // Awake時点で重複を切る（NetworkSpawnより前に動く）
        if (Instance != null && Instance != this)
        {
            // すでにSingletonがいるなら自分は退場
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (useDontDestroyOnLoad)
        {
            // ルートごと保持（Prefab直下にAudioSource等があってもまとめて保持される）
            DontDestroyOnLoad(transform.root.gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 万一ネットワークSpawn後に重複が出てもここで潰す
        if (Instance != null && Instance != this)
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                // 先にDespawnしてから破棄
                if (IsServer) NetworkObject.Despawn(true);
            }
            Destroy(gameObject);
            return;
        }

        // 念のため再確定
        Instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        // Domain Reload無効対策：破棄されたら参照を解放
        if (Instance == this) Instance = null;
    }
}

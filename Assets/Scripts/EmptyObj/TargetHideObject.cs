using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // UnityEventのため
using Unity.Netcode;
// using Mono.Cecil.Cil; // もし未使用であれば削除を検討

// UnityEvent2 がカスタムクラスなら、その using も必要
// public class UnityEvent2 : UnityEvent {} // 例: もし単純なUnityEventのエイリアスなら

public class TargetHideObject : NetworkBehaviour
{
    //どこに隠すか場所を指定！
    public Transform HideBeyondPos;
    //隠れた後その場所から出るときにどこに出るか。
    public Transform UnHideBeyondPos;

    //誰かが隠れているかどうか。
    public NetworkVariable<bool> SomeoneHided = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //隠れるタイプは何なのか。
    public enum HideType
    {
        Stand,
        Crouch,
        FaceDown
    };
    [Header("隠れるタイプ")]
    [Tooltip("Stand: 立った状態 \n Crouch: 座った状態 \n FaceDown: うつ伏せ状態")]
    public HideType hide_type; // このHideObjectの隠れ方タイプ (Inspectorで設定)

    // public Vector3 TargetObjectOfAngle; // 使用箇所不明のためコメントアウトも検討

    [Header("Playerの子オブジェクトにあるAudioDatasに設定されてるaudioClipのKeyを指定。")]
    [SerializeField] private string AudioClipKey;

    [SerializeField] private UnityEvent2 HideAddEvent; // UnityEvent2 から UnityEvent に変更（標準機能）
    [SerializeField] private UnityEvent2 UnHideAddEvent;
    [SerializeField] private UnityEvent LocalHideAddEvent;
    [SerializeField] private UnityEvent LocalUnHideAddEvent;

    // これらの参照はローカルプレイヤーに依存するため、扱いが難しい
    // 各クライアントが自身のプレイヤーの参照を持つ必要がある
    private AudioSource _localPlayerHideActionAudioSource;
    private AudioDatas _localPlayerHideActionAudioDatas;
    // private SubNoticedUIController _localPlayerSubNoticedUiController; // これはPlayerStatusが持つべき

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnLocalPlayerSpawn;
        SomeoneHided.OnValueChanged += OnSomeoneHidedChanged;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnLocalPlayerSpawn;
        if (SomeoneHided != null)
        {
            SomeoneHided.OnValueChanged -= OnSomeoneHidedChanged;
        }
    }

    private void InitOnLocalPlayerSpawn(PlayerStatus localPlayerStatus)
    {
        // ローカルプレイヤーがスポーンしたときに、そのプレイヤーのコンポーネントを参照する
        // これにより、効果音などがローカルプレイヤーに対して正しく動作するようになる
        if (localPlayerStatus != null)
        {
            _localPlayerHideActionAudioDatas = localPlayerStatus.HideActionAudioDatas;
            if (_localPlayerHideActionAudioDatas != null)
            {
                _localPlayerHideActionAudioSource = _localPlayerHideActionAudioDatas.audioSource;
            }
        }
    }

    private void OnSomeoneHidedChanged(bool previousValue, bool newValue)
    {
        // このオブジェクトに誰か隠れた/出た場合に全クライアントで呼ばれる
        // 例えば、オブジェクトの見た目を変更する（扉が閉まるなど）処理をここで行える
        Debug.Log($"TargetHideObject '{this.gameObject.name}' SomeoneHided changed from {previousValue} to {newValue}");
        if (newValue) // 誰かが隠れた
        {
            HideAddEvent.Invoke(); // 全クライアントで実行されるイベント
        }
        else // 誰も隠れていない
        {
            UnHideAddEvent.Invoke(); // 全クライアントで実行されるイベント
        }
    }


    // ──────────────────────────────────
    // サーバー RPC：プレイヤーがこのオブジェクトに隠れることを要求
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)] // どのクライアントからでも呼び出せる
    public void HideRequestServerRpc(ulong playerNetId, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] HideRequestServerRpc for object {this.gameObject.name} from playerNetId {playerNetId}");
        if (SomeoneHided.Value)
        {
            Debug.LogWarning($"[Server] Player {playerNetId} tried to hide in {this.gameObject.name}, but it's already occupied.");
            // ここでリクエスト元クライアントに「既に使われている」と通知することも可能
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out NetworkObject playerNetworkObj))
        {
            HideTargetOnServer(playerNetworkObj.gameObject); // サーバーでプレイヤーを移動、状態変更

            // 隠れた本人（リクエスト元）にローカルな処理を実行させるためのClientRpc
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerNetworkObj.OwnerClientId } }
            };
            HidingPlayerLocalActionsClientRpc(clientRpcParams);
        }
        else
        {
            Debug.LogError($"[Server] Player with NetId {playerNetId} not found for HideRequest.");
        }
    }

    // ──────────────────────────────────
    // サーバー RPC：プレイヤーがこのオブジェクトから出ることを要求
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void UnHideRequestServerRpc(ulong playerNetId, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] UnHideRequestServerRpc for object {this.gameObject.name} from playerNetId {playerNetId}");
        if (!SomeoneHided.Value)
        {
            Debug.LogWarning($"[Server] Player {playerNetId} tried to unhide from {this.gameObject.name}, but it's not occupied.");
            // 状態がおかしい場合の処理
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out NetworkObject playerNetworkObj))
        {
            UnHideTargetOnServer(playerNetworkObj.gameObject); // サーバーでプレイヤーを移動、状態変更

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerNetworkObj.OwnerClientId } }
            };
            UnhidingPlayerLocalActionsClientRpc(clientRpcParams);
        }
        else
        {
            Debug.LogError($"[Server] Player with NetId {playerNetId} not found for UnHideRequest.");
        }
    }

    // ──────────────────────────────────
    // サーバーでのみ実行: プレイヤーを隠す処理
    // ──────────────────────────────────
    private void HideTargetOnServer(GameObject playerObject)
    {
        playerObject.transform.position = HideBeyondPos.position;
        // 必要ならプレイヤーの向きも設定
        // playerObject.transform.rotation = HideBeyondPos.rotation; // Quaternion.Euler(TargetObjectOfAngle) など
        SomeoneHided.Value = true; // NetworkVariableを更新 -> 全クライアントに同期
    }

    // ──────────────────────────────────
    // サーバーでのみ実行: プレイヤーを出す処理
    // ──────────────────────────────────
    private void UnHideTargetOnServer(GameObject playerObject)
    {
        playerObject.transform.position = UnHideBeyondPos.position;
        SomeoneHided.Value = false; // NetworkVariableを更新 -> 全クライアントに同期
    }

    // ──────────────────────────────────
    // クライアント RPC：隠れた本人向けのローカル処理
    // ──────────────────────────────────
    [ClientRpc]
    private void HidingPlayerLocalActionsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // このRPCは指定されたクライアント（隠れた本人）でのみ実行される
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] HidingPlayerLocalActionsClientRpc for object {this.gameObject.name}");
        if (_localPlayerHideActionAudioSource != null && !string.IsNullOrEmpty(AudioClipKey) &&
            _localPlayerHideActionAudioDatas.audioClipDatasWithKey.ContainsKey(AudioClipKey))
        {
            _localPlayerHideActionAudioSource.PlayOneShot(
                _localPlayerHideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        }
        LocalHideAddEvent.Invoke(); // RaycastPermit = false など
    }

    // ──────────────────────────────────
    // クライアント RPC：出た本人向けのローカル処理
    // ──────────────────────────────────
    [ClientRpc]
    private void UnhidingPlayerLocalActionsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // このRPCは指定されたクライアント（出た本人）でのみ実行される
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] UnhidingPlayerLocalActionsClientRpc for object {this.gameObject.name}");
        if (_localPlayerHideActionAudioSource != null && !string.IsNullOrEmpty(AudioClipKey) &&
           _localPlayerHideActionAudioDatas.audioClipDatasWithKey.ContainsKey(AudioClipKey))
        {
            _localPlayerHideActionAudioSource.PlayOneShot(
                _localPlayerHideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        }
        LocalUnHideAddEvent.Invoke(); // RaycastPermit = true など
    }

}
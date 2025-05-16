// DrawerManager.cs
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DrawerManager : NetworkBehaviour
{
    [Header("子オブジェクトの ItemBoxOpenCloseController を順番に登録")]
    [SerializeField] private List<ItemBoxOpenCloseController> drawers;

    // 各引き出しの開閉状態を管理
    private bool[] isOpenStates;

    public override void OnNetworkSpawn()
    {
        // シーン開始時に状態配列を初期化（全てクローズ状態）
        isOpenStates = new bool[drawers.Count];
    }

    // クライアントから「このインデックスの引き出しをトグルして！」を受け取る
    [ServerRpc(RequireOwnership = false)]
    public void RequestToggleBoxServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (index < 0 || index >= drawers.Count) return;
        // アニメ中なら無視
        if (!drawers[index].AnimationCompleted) return;

        // 開閉状態を反転して
        isOpenStates[index] = !isOpenStates[index];

        // 反転後の状態に応じて対応する ClientRpc を投げる
        if (isOpenStates[index])
            OpenDrawerClientRpc(index);
        else
            CloseDrawerClientRpc(index);
    }

    // 全クライアント上で「開く」ローカル処理を走らせる
    [ClientRpc]
    private void OpenDrawerClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        drawers[index].LocalOpen();
    }

    // 全クライアント上で「閉じる」ローカル処理を走らせる
    [ClientRpc]
    private void CloseDrawerClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        drawers[index].LocalClose();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

/// <summary>
/// 扉への攻撃判定と破壊処理をまとめたコンポーネント
/// （Netcode for GameObjects 前提でサーバー側で実行）
/// </summary>
public class DoorAttackedController : NetworkBehaviour
{
    // 扉を壊すのに必要な攻撃回数（初期値 3 回）
    [SerializeField] private float need_break_count = 3f;

    // 現在の攻撃回数
    [SerializeField] private float break_count_now = 0f;

    // 扉のパネルを構成する GameObject 群
    public List<GameObject> Doors;

    /// <summary>
    /// 扉にダメージを与える  
    /// 戻り値 true ならこの呼び出しで壊れた
    /// </summary>
    public bool DoorAttacked(int PlusAttackCount = 1)
    {
        // ここはサーバー側でのみ実行
        Debug.Log($"ドアが攻撃された: 現在のカウント {break_count_now}");
        break_count_now += PlusAttackCount;

        if (break_count_now >= need_break_count)
        {
            DoorBrokenCompletedForAllClientsClientRpc();
            // gameObject.SetActive(false); // 必要ならオブジェクトを無効化
            return true;
        }
        else
        {
            // まだ壊れていない
            return false;
        }
    }

    /// <summary>
    /// 全クライアントに「ドアが壊れた」ことを同期させる
    /// </summary>
    [ClientRpc]
    private void DoorBrokenCompletedForAllClientsClientRpc()
    {
        Debug.Log("ドアが完全に破壊された（全クライアント同期済み）");

        foreach (GameObject door in Doors)
        {
            // Rigidbody を追加して物理挙動を付与
            Rigidbody rb = door.AddComponent<Rigidbody>();
            if (rb == null) continue;

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // プレイヤーと衝突させたくない場合は専用レイヤーへ
            door.layer = LayerMask.NameToLayer("WithoutCharacter");

            // 見た目用に少し回転させて吹き飛んだ感じを出す
            door.transform.Rotate(30f, 30f, 30f);
        }
    }
}

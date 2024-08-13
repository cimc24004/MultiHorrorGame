using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
    [ExecuteInEditMode]
    public class CanCheckUIDIsplayRangeControl : SerializedMonoBehaviour
    {


        [LabelText("クロスヘアから発せられるレイの距離から範囲を計算しコライダーサイズをリサイズするボタン")]
        [Button]
        public void ResizeColliderOnCrossHairDistance(PlayerStatus player_status)
        {
            GameObject parent = this.transform.parent.gameObject;
            Vector3 ParentScale = parent.gameObject.transform.lossyScale;
            float plusX;
            float plusZ;
            BoxCollider can_check_display_range_collider = this.gameObject.GetComponent<BoxCollider>();
            float Distance = player_status.cross_hair_event.RaycastDistance;

            plusX = ParentScale.x < 1 ?  (1 / ParentScale.x) : (ParentScale.x);
            plusZ = ParentScale.y < 1 ? (1 / ParentScale.y) : (ParentScale.y);
            can_check_display_range_collider.size = new Vector3(((Distance * 2) + 1) * plusX, 1, ((Distance * 2) + 1) * plusZ);
            can_check_display_range_collider.center = Vector3.zero;
        }
    }
#endif
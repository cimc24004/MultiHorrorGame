using UnityEngine;

[ExecuteInEditMode] // エディタ上でも適用したい場合は付与
public class IgnoreParentScale : MonoBehaviour
{
    [SerializeField] private Vector3 initialLocalScale;

    private void Start()
    {
    }

    private void LateUpdate()
    {
        // 親階層をすべて遡って得られるトータルのスケールを逆数にし、初期ローカルスケールに掛ける
        Vector3 totalParentScale = GetTotalParentScale(transform.parent);
        transform.localScale = Vector3.Scale(initialLocalScale, InvertVector(totalParentScale));
    }

    private Vector3 GetTotalParentScale(Transform parent)
    {
        if (parent == null) return Vector3.one;
        Vector3 pScale = parent.localScale;
        return Vector3.Scale(pScale, GetTotalParentScale(parent.parent));
    }

    private Vector3 InvertVector(Vector3 vec)
    {
        // 0除算を回避しつつ安全に逆数をとる
        return new Vector3(
            Mathf.Abs(vec.x) < 1e-5f ? 1 : 1f / vec.x,
            Mathf.Abs(vec.y) < 1e-5f ? 1 : 1f / vec.y,
            Mathf.Abs(vec.z) < 1e-5f ? 1 : 1f / vec.z
        );
    }
}

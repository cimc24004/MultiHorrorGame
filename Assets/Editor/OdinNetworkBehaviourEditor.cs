// Assets/Editor/OdinNetworkBehaviourEditor.cs
using Sirenix.OdinInspector.Editor;
using Unity.Netcode;
using UnityEditor;

// 「true」を渡すことで、NetworkBehaviour を継承する全サブクラスにも適用される
[CustomEditor(typeof(NetworkBehaviour), true)]
public class OdinNetworkBehaviourEditor : OdinEditor { }

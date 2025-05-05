using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class AudioDatas : SerializedMonoBehaviour
{
    [SerializeField] public Dictionary<string, AudioClip> audioClipDatasWithKey = new Dictionary<string, AudioClip>();
    [SerializeField] public AudioSource audioSource;
}

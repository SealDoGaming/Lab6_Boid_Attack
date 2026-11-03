using UnityEngine;

[CreateAssetMenu(fileName = "WeightedAudio", menuName = "Scriptable Objects/WeightedAudio")]
public class WeightedAudio : ScriptableObject
{
    public AudioClip audioClip;

    public float weight = 1.0f;
}

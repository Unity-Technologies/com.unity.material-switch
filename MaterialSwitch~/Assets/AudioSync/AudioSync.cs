using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(TimelineAsset))]
public class AudioSync : MonoBehaviour
{
    public string audioGroupName = "Sound";
    public string destinationGroupName = "_AudioSync";
    public string templateTrackName = "Template";
    public float[] dBThresholdsToMaterialSwitch;

    public const double DiscreteStep = 1.0 / 24.0;
}
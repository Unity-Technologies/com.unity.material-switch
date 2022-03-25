using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Timeline;
using Unity.MaterialSwitch;
using UnityEditor.Timeline;
using UnityEngine.Audio;
using UnityEngine.Playables;

[CustomEditor(typeof(AudioSync))]
public class AudioSyncEditor : Editor
{
    private AudioSync _audioSync = null;
    private TimelineAsset _timelineAsset = null;

    private void DumpDebugInfo()
    {
        if (!SearchSourceAndDestinationTrack(out var audioTrackQueue, out var materialSwitchTrackQueue,
                out var materialSwitchTemplateQueue))
        {
            return;
        }

        foreach (var materialSwitchTrack in materialSwitchTemplateQueue)
        {
            foreach (var clip in materialSwitchTrack.GetClips())
            {
                var asset = clip.asset as MaterialSwitchClip;
                if (asset == null)
                {
                    return;
                }

                Debug.Log("-----");
                Debug.Log(clip.displayName);

                // foreach (var mp in asset.materialPropertiesList)
                // {
                //     foreach (var textureProperty in mp.textureProperties)
                //     {
                //         if (!textureProperty.overrideBaseValue)
                //         {
                //             continue;
                //         }
                //
                //         Debug.Log(textureProperty.propertyName);
                //         Debug.Log(textureProperty.overrideBaseValue);
                //         Debug.Log(textureProperty.baseValue);
                //         Debug.Log(textureProperty.targetValue);
                //     }
                // }
            }
        }

        foreach (var materialSwitchTrack in materialSwitchTrackQueue)
        {
            foreach (var clip in materialSwitchTrack.GetClips())
            {
                var materialSwitchClip = clip.asset as MaterialSwitchClip;
                if (materialSwitchClip == null)
                {
                    return;
                }

                Debug.Log("-----");
                Debug.Log(materialSwitchClip.name);
            //     Debug.Log(materialSwitchClip.materialPropertiesList.Count);
            //     foreach (var mp in materialSwitchClip.materialPropertiesList)
            //     {
            //         foreach (var textureProperty in mp.textureProperties)
            //         {
            //             if (textureProperty.targetValue == textureProperty.baseValue)
            //             {
            //                 continue;
            //             }
            //
            //             Debug.Log(textureProperty.propertyName);
            //             Debug.Log(textureProperty.overrideBaseValue);
            //             Debug.Log(textureProperty.baseValue);
            //             Debug.Log(textureProperty.targetValue);
            //         }
            //     }
            }
        }
    }

    private void Bake()
    {
        if (!SearchSourceAndDestinationTrack(out var audioTrackQueue, out var materialSwitchTrackQueue,
                out var materialSwitchTemplateQueue))
        {
            return;
        }

        foreach (var audioTrack in audioTrackQueue)
        {
            var materialSwitchTrack = materialSwitchTrackQueue.Dequeue();
            var materialSwitchTemplateTrack = materialSwitchTemplateQueue.Dequeue();

            var clipTemplates = new Dictionary<string, TimelineClip>();
            foreach (var clip in materialSwitchTemplateTrack.GetClips())
            {
                clipTemplates.Add(clip.displayName, clip);
            }

            foreach (var clip in materialSwitchTrack.GetClips())
            {
                materialSwitchTrack.DeleteClip(clip);
            }

            foreach (var audioTimelineClip in audioTrack.GetClips())
            {
                var audioAsset = audioTimelineClip.asset as AudioPlayableAsset;
                var audioClip = audioAsset?.clip;
                var audioData = new float[audioClip.samples * audioClip.channels];
                audioClip.GetData(audioData, 0);
                var lastLevel = 0;
                var lastLevelChangedClock = audioTimelineClip.start;
                for (var t = 0.0; t < audioTimelineClip.duration; t += AudioSync.DiscreteStep)
                {
                    var sampleIndex = (int)((t + audioTimelineClip.clipIn) * (double)audioClip.frequency);
                    sampleIndex *= audioClip.channels;
                    var dB = Decibel(t, audioClip.frequency, audioTimelineClip, audioData, sampleIndex);

                    var level = 0;
                    foreach (var f in _audioSync.dBThresholdsToMaterialSwitch)
                    {
                        if (f < dB)
                        {
                            level++;
                        }
                    }

                    if (level != lastLevel)
                    {
                        var clock = t + audioTimelineClip.start;
                        //lastlevel->level
                        //0->1生成しない
                        //1->0生成する
                        if (0 < lastLevel)
                        {
                            if (CreateClip(materialSwitchTrack, clipTemplates["1"], lastLevelChangedClock, clock,
                                    lastLevel)) return;
                        }

                        lastLevelChangedClock = clock;
                    }

                    lastLevel = level;
                }
            }
        }

        Debug.Log("Baked.");

        float Decibel(double t, double frequency, TimelineClip clip, float[] floats, int sampleIndex)
        {
            float sum = 0;
            var loopNum = (int)(AudioSync.DiscreteStep * (float)frequency);
            var sampleNum = 0;
            for (var i = 0; i < loopNum; i++)
            {
                var indexToSample = sampleIndex + i;
                if (indexToSample < 0 || floats.Length <= indexToSample)
                {
                    continue;
                }

                sum += floats[indexToSample] * floats[indexToSample];
                sampleNum++;
            }

            if (sampleNum == 0)
            {
                return 0;
            }

            var rms = Mathf.Sqrt(sum / sampleNum);
            var dB = Mathf.Log(rms, 20.0f);
            return dB;
        }

        bool CreateClip(MaterialSwitchTrack targetTrack, TimelineClip templateClip, double lastLevelChangedClock,
            double currentClock,
            int volumeLevel)
        {
            if (templateClip == null)
            {
                return true;
            }

            
            var defaultClip = targetTrack.CreateDefaultClip();
            defaultClip.start = lastLevelChangedClock;
            defaultClip.duration = currentClock - lastLevelChangedClock;
            defaultClip.displayName = volumeLevel.ToString();

            var asset = defaultClip.asset as MaterialSwitchClip;
            if (asset == null)
            {
                Debug.LogError("asset isn't MaterialSwitchClip.");
                return true;
            }

            MaterialSwitchUtility.InitMaterialSwitchClip(defaultClip);
            


            foreach (var sm in materialPropertyGroup.sharedMaterials)
            {
                var mp = MaterialSwitchUtility.CreateMaterialProperties(sm);

                var templateMaterialSwitchClip = templateClip.asset as MaterialSwitchClip;
                foreach (var tmp in templateMaterialSwitchClip.materialPropertiesList)
                {
                    foreach (var textureProperty in tmp.textureProperties)
                    {
                        if (!textureProperty.overrideBaseValue)
                        {
                            continue;
                        }

                        mp.textureProperties.Add(new TextureProperty()
                        {
                            displayName = textureProperty.displayName,
                            propertyName = textureProperty.propertyName,
                            propertyId = textureProperty.propertyId,
                            targetValue = textureProperty.targetValue,
                            overrideBaseValue = true
                        });
                        // Debug.Log(textureProperty.propertyName);
                        // Debug.Log(textureProperty.overrideBaseValue);
                        // Debug.Log(textureProperty.baseValue);
                        // Debug.Log(textureProperty.targetValue);
                    }
                }

                asset.materialPropertiesList.Add(mp);
            }

            return false;
        }
    }

    private void DeleteGeneratedClips()
    {
        SearchSourceAndDestinationTrack(out var audioTrackQueue, out var materialSwitchTrackQueue,
            out var materialSwitchTemplateQueue);

        foreach (var materialSwitchTrack in materialSwitchTrackQueue)
        {
            foreach (var clip in materialSwitchTrack.GetClips())
            {
                materialSwitchTrack.DeleteClip(clip);
            }
        }

        Debug.Log("Deleted.");
    }

    private bool SearchSourceAndDestinationTrack(out Queue<AudioTrack> audioTrackQueue,
        out Queue<MaterialSwitchTrack> materialSwitchTrackQueue, out Queue<MaterialSwitchTrack>
            materialSwitchTemplateQueue)
    {
        audioTrackQueue = new Queue<AudioTrack>();
        materialSwitchTrackQueue = new Queue<MaterialSwitchTrack>();
        materialSwitchTemplateQueue = new Queue<MaterialSwitchTrack>();

        TrackAsset audioTrackGroup = null;
        TrackAsset audioSyncTrackGroup = null;
        foreach (var rt in _timelineAsset.GetRootTracks())
        {
            if (string.CompareOrdinal(rt.name, _audioSync.audioGroupName) == 0)
            {
                Debug.Log("AudioGroupName matched");
                audioTrackGroup = rt;
            }

            if (string.CompareOrdinal(rt.name, _audioSync.destinationGroupName) == 0)
            {
                Debug.Log("AudioSyncGroupName matched");
                audioSyncTrackGroup = rt;
            }
        }

        if (audioTrackGroup == null)
        {
            Debug.LogError("AudioGroupName didnt match");
            return false;
        }

        if (audioSyncTrackGroup == null)
        {
            Debug.LogError("AudioSyncGroupName didnt match");
            return false;
        }

        foreach (var ta in audioTrackGroup.GetChildTracks())
        {
            if (ta is AudioTrack)
            {
                audioTrackQueue.Enqueue(ta as AudioTrack);
            }
        }

        foreach (var ta in audioSyncTrackGroup.GetChildTracks())
        {
            if (ta is MaterialSwitchTrack)
            {
                if (string.CompareOrdinal(ta.name, _audioSync.templateTrackName) == 0)
                {
                    materialSwitchTemplateQueue.Enqueue(ta as MaterialSwitchTrack);
                }
                else
                {
                    materialSwitchTrackQueue.Enqueue(ta as MaterialSwitchTrack);
                }
            }
        }

        return true;
    }

    void OnEnable()
    {
        _audioSync = target as AudioSync;
        _timelineAsset = _audioSync.GetComponent<PlayableDirector>()?.playableAsset as TimelineAsset;
        if (_timelineAsset == null)
        {
            Debug.LogError("TimelineAsset not found.");
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Bake"))
        {
            Bake();
        }

        if (GUILayout.Button("DeleteBakedClips"))
        {
            DeleteGeneratedClips();
        }

        if (GUILayout.Button("Debug Info"))
        {
            DumpDebugInfo();
        }
    }
}
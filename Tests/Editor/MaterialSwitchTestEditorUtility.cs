using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.MaterialSwitch.EditorTests
{
internal static class MaterialSwitchEditorTestUtility
{
    private static PlayableDirector CreateDirectorWithTimelineAsset(string candidatePath, 
        out TimelineAsset timelineAsset) 
    {
        timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
        PlayableDirector director = new GameObject("Director").AddComponent<PlayableDirector>();
        Assert.IsNotNull(timelineAsset);

        director.playableAsset = timelineAsset;
        return director;
    }
    
}

} //end namespace
using System.Collections;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch.EditorTests
{
internal static class EditorTestUtility
{
        
//----------------------------------------------------------------------------------------------------------------------
    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static void Destroy(Object obj, bool forceImmediate = false, bool allowDestroyingAssets = false) {
        if (!Application.isPlaying || forceImmediate) {
            Object.DestroyImmediate(obj,allowDestroyingAssets);                        
        } else {
            Object.Destroy(obj);            
        }
    }
//----------------------------------------------------------------------------------------------------------------------
    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static IEnumerator WaitForFrames(int numFrames) {
        for (int i = 0; i < numFrames; ++i) {
            yield return null;
            
        }        
        Undo.IncrementCurrentGroup();
    }        

    
}

} //end namespace

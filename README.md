# Material Switch

A selection group is a collection of gameobjects.

We want to change material properties for all materials being used in a selection group, using a timeline clip which contains a collection of target materials with modifications by the user.

To enable this, we add a MaterialPropertyGroup component to the Runtime.SelectionGroup gameobject in the scene. 

The MaterialPropertyGroup component stores a reference to all the sharedMaterials used by the selection group, then creates a copy of all those materials into an array called 'originalMaterials'. This component has methods for restoring the original material properties, and lerping the array of sharedMaterials towards an array of target materials. 

The timeline needs to have a MaterialSwitchTrack. A MaterialProperyGroup component is assigned into the property slot on the track.

When a MaterialSwitchClip is added to the track, the ClipEditor for the track creates copies of the originalMaterials from the MaterialPropertyGroup component, and assigns those copies to the clip, and adds them as assets to the clip using AssetDatabase.AddObjectToAsset.

When the user selects the clip, the inspector shows the list of all materials on the clip, which can then be adjusted by the user. When the clip is played, the MaterialPropertyGroup.LerpTowards method is used to lerp the sharedMaterials towards the target materials on the clip. Note, this _will change the material values for the selection group on disk_. To reset the sharedMaterials to the original values, scrub to an empty area of the timeline, or right click the MaterialPropertyGroup component and choose 'Restore Original Materials'.

Unity does not support texture interpolation in Material.Lerp, however we could add this feature manually in MaterialPropertyGroup.LerpTowards if required in our use-cases.

Known Issues:
- Any objects that are not part of the selection group that use the same shared materials from the selection group, will also be changed. To mitigate this problem, each selection group should not use shared materials with other groups. This could probably be enforced or made easier to configure using code, however this would introduce a new batch for each selection group, which may not be desirable.
- If a material is updated, or a model changes materials, the MaterialPropertyGroup will no longer reflect the intended 'originalMaterial' and will need to be reset, as well as any MaterialSwitchClips and Tracks which use that MaterialPropertyGroup. 



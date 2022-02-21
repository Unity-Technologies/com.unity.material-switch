using System;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    [CustomEditor(typeof(MaterialSwitchClip))]
    [CanEditMultipleObjects]
    internal class MaterialSwitchClipEditor : Editor
    {
        bool showTextureProperties;
        private static MaterialSwitchClip copySource;
        private static int sourceIndex;
        private string errorMessage = null;

        private HashSet<Material> activeMaterials = null;
        private GUIContent RemoveButtonGuiContent;
        private GUIContent SettingsGuiContent;

        void UpdateSampledColors()
        {
            var clip          = target as MaterialSwitchClip;
            var globalTexture = clip.globalMaterialProperties.texture;
            foreach (var map in clip.materialPropertiesList)
            {
                var textureToUse = map.texture == null ? globalTexture : map.texture;
                if (textureToUse == null) continue;
                if(!textureToUse.isReadable) continue;
                foreach (var c in map.colorProperties)
                {
                    c.targetValue = textureToUse.GetPixel((int) c.uv.x, (int) c.uv.y);
                }
            }
        }

        void HandleContextClick(SerializedProperty property, int targetIndex)
        {
            var e = Event.current;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Settings"), false, () =>
            {
                copySource = Instantiate(target) as MaterialSwitchClip;
                sourceIndex = targetIndex;
            });
            if(copySource == null)
                menu.AddDisabledItem(new GUIContent("Paste Settings"));
            else
                menu.AddItem(new GUIContent("Paste Settings"), false, () =>
                {
                    var targetClip = target as MaterialSwitchClip;
                    Undo.RecordObject(targetClip, "Paste");
                    //negative targetIndex is reserved for global properties
                    if (targetIndex < 0)
                    {
                        string json;
                        if (sourceIndex < 0)
                            json = EditorJsonUtility.ToJson(copySource.globalMaterialProperties);
                        else
                            json = EditorJsonUtility.ToJson(copySource.materialPropertiesList[sourceIndex]);
                        EditorJsonUtility.FromJsonOverwrite(json, targetClip.globalMaterialProperties);
                    }
                    else
                    {
                        //preserve material reference, this is not normally changed.
                        var oldMaterial = targetClip.materialPropertiesList[targetIndex].material;
                        string json;
                        if (sourceIndex < 0)
                            json = EditorJsonUtility.ToJson(copySource.globalMaterialProperties);
                        else
                            json = EditorJsonUtility.ToJson(copySource.materialPropertiesList[sourceIndex]);

                        EditorJsonUtility.FromJsonOverwrite(json, targetClip.materialPropertiesList[targetIndex]);
                        targetClip.materialPropertiesList[targetIndex].material = oldMaterial;
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                });
            
            menu.ShowAsContext();
            e.Use();
        }

        private void OnEnable()
        {
            RemoveButtonGuiContent = EditorGUIUtility.IconContent("d_winbtn_win_close");
            RemoveButtonGuiContent.tooltip = "Remove Property";
            SettingsGuiContent = EditorGUIUtility.IconContent("d_SettingsIcon");
            SettingsGuiContent.tooltip = "Settings";
            
            
            errorMessage = null;
            //when the editor is enabled, get the target clip and make sure it is up to date.
            
            PlayableDirector inspectedDirector = TimelineEditor.inspectedDirector;
            if (inspectedDirector == null)
            {
                errorMessage = "Could not find Timeline Director.";
                return;
            }

            var selectedClip = TimelineEditor.selectedClip;
            if(selectedClip == null) 
            {
                errorMessage = "Could not find Selected Clip.";
                return;
            }
            
            var track = selectedClip.GetParentTrack();
            var selectionGroup = inspectedDirector.GetGenericBinding(track) as SelectionGroups.SelectionGroup;
            if (selectionGroup == null)
            {
                errorMessage = "No Selection Group is bound to the Track.";
                return;
            }
             
            if (!selectionGroup.TryGetComponent(out MaterialGroup materialGroup))
            {
                materialGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
            }
            materialGroup.CollectMaterials();
            
            var asset = target as MaterialSwitchClip;
            
            if (asset.globalMaterialProperties == null || asset.globalMaterialProperties.needsUpdate)
            {
                asset.globalMaterialProperties =
                    MaterialSwitchUtility.CreateMaterialProperties(materialGroup.sharedMaterials);
            }

            activeMaterials = new HashSet<Material>(materialGroup.sharedMaterials);
            var storedMaterials = new HashSet<Material>(from i in asset.materialPropertiesList select i.material);
            var missingMaterials = activeMaterials.Except(storedMaterials).ToArray();
            if (missingMaterials.Length > 0)
            {
                errorMessage = $"Created {missingMaterials.Length} new material maps. These will be saved in the clip asset.";
                foreach (var material in missingMaterials)
                {
                    var materialProperties = MaterialSwitchUtility.CreateMaterialProperties(material);
                    asset.materialPropertiesList.Add(materialProperties);
                }
                EditorUtility.SetDirty(asset);
            }

        }

        public override void OnInspectorGUI()
        {
            
            serializedObject.Update();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);
            }
            
            GUILayout.BeginVertical("box");
            
            var globalPalettePropertyMap = serializedObject.FindProperty(nameof(MaterialSwitchClip.globalMaterialProperties));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Global Properties");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(SettingsGuiContent, EditorStyles.toolbarDropDown))
            {
                //negative targetIndex is reserved for global properties
                HandleContextClick(globalPalettePropertyMap, -1);
            }
            GUILayout.EndHorizontal();
            
            //negative targetIndex is reserved for global properties
            DrawPalettePropertyMapUI(globalPalettePropertyMap, null, -1);                        

            EditorGUI.indentLevel += 1;
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.BeginVertical("box");
            GUILayout.Label("Per Material Properties");
            
            var materialPropertiesList = serializedObject.FindProperty(nameof(MaterialSwitchClip.materialPropertiesList));
            for (var i = 0; i < materialPropertiesList.arraySize; i++)
            {
                var property = materialPropertiesList.GetArrayElementAtIndex(i);
                var materialProperty = property.FindPropertyRelative(nameof(MaterialProperties.material));
                var material = materialProperty.objectReferenceValue as Material;
                if (activeMaterials.Contains(material))
                {
                    DrawPalettePropertyMapUI(property, globalPalettePropertyMap, i);
                }
            }
            GUILayout.EndVertical();

            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateSampledColors();
                serializedObject.ApplyModifiedProperties();
            }
        }


        private void DrawPalettePropertyMapUI(SerializedProperty ppm, SerializedProperty globalPalettePropertyMap, int index)
        {
            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel--;
            if (globalPalettePropertyMap != null)
            {
                //This is a per material ppm, so draw the material field.
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative(nameof(MaterialProperties.material)), GUILayout.MinWidth(384));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(SettingsGuiContent, EditorStyles.toolbarDropDown))
                {
                    HandleContextClick(ppm, index);
                }
                GUILayout.EndHorizontal();
                
            }

            var textureProperty = ppm.FindPropertyRelative(nameof(MaterialProperties.texture));
            var globalTextureProperty = globalPalettePropertyMap?.FindPropertyRelative(nameof(MaterialProperties.texture));
            
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(textureProperty, new GUIContent("Palette Texture"));
            if (textureProperty.objectReferenceValue != null)
            {
                var t = textureProperty.objectReferenceValue as Texture2D;
                if (!t.isReadable)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Texture is not marked as readable!", MessageType.Error);
                    if (GUILayout.Button("Fix"))
                        MakeTextureReadable(t);
                    GUILayout.EndHorizontal();
                }
            }


            DrawPropertyOverrideList(ppm, "showCoords", "Color Properties", "colorProperties",
                (itemProperty) =>
                {
                    var displayNameProperty = itemProperty.FindPropertyRelative(nameof(ColorProperty.displayName));
                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{displayNameProperty.stringValue}");
                    GUILayout.FlexibleSpace();
                    ShowRemoveOverrideButton(itemProperty);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Sampled Color");
                    var texture = ppm.FindPropertyRelative(nameof(MaterialProperties.texture)).objectReferenceValue as Texture2D;
                    var globalPaletteTexture = globalTextureProperty?.objectReferenceValue as Texture2D;
                    if (texture == null && globalPaletteTexture == null)
                    {
                        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(ColorProperty.targetValue)),
                            GUIContent.none);
                    }
                    else
                    {
                        var rect = GUILayoutUtility.GetRect(18, 18);
                        var targetValueProperty = itemProperty.FindPropertyRelative(nameof(ColorProperty.targetValue));
                        EditorGUI.DrawRect(rect, targetValueProperty.colorValue);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(ColorProperty.uv)));
                        var textureToUse = texture == null ? globalPaletteTexture : texture;
                        GUI.enabled = textureToUse != null;
                        if (GUILayout.Button("Pick") || GUI.Button(rect, GUIContent.none, "label"))
                        {
                            rect = GUIUtility.GUIToScreenRect(rect);
                            CoordPickerWindow.Open(this, textureToUse, itemProperty, rect);
                        }
                    }

                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                });


            DrawPropertyOverrideList(ppm, "showTextures", "Texture Properties", "textureProperties");
            DrawPropertyOverrideList(ppm, "showFloats", "Float Properties", "floatProperties", (itemProperty) =>
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    $"{itemProperty.FindPropertyRelative(nameof(FloatProperty.displayName)).stringValue}");
                GUILayout.FlexibleSpace();
                ShowRemoveOverrideButton(itemProperty);
                GUILayout.EndHorizontal();
                //EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(FloatProperty.baseValue)));
                var limits = itemProperty.FindPropertyRelative(nameof(RangeProperty.rangeLimits));
                if (limits != null)
                {
                    var targetValueProperty = itemProperty.FindPropertyRelative(nameof(FloatProperty.targetValue));
                    var minmax = limits.vector2Value;
                    var originalValue = targetValueProperty.floatValue;
                    var newValue = EditorGUILayout.Slider(originalValue, minmax.x, minmax.y);
                    if (originalValue != newValue)
                    {
                        targetValueProperty.floatValue = newValue;
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(FloatProperty.targetValue)));
                }

                GUILayout.EndVertical();
            });


            GUILayout.EndVertical();
        }

        private void ShowRemoveOverrideButton(SerializedProperty itemProperty)
        {
            if (GUILayout.Button(RemoveButtonGuiContent, EditorStyles.toolbarButton))
            {
                var overrideBaseValueProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.overrideBaseValue));
                overrideBaseValueProperty.boolValue = false;
            }
        }

        private void DrawPropertyOverrideList(SerializedProperty ppm, string togglePropertyPath, string heading,
            string arrayPropertyPath, System.Action<SerializedProperty> guiMethod = null)
        {
            var showPropertyListToggle = ppm.FindPropertyRelative(togglePropertyPath);
            showPropertyListToggle.boolValue = EditorGUILayout.Foldout(showPropertyListToggle.boolValue, heading);
            if (showPropertyListToggle.boolValue)
            {
                var propertyList = ppm.FindPropertyRelative(arrayPropertyPath);
                if (propertyList != null)
                {
                    GUILayout.BeginVertical("box");
                    var menu = new GenericMenu();

                    for (var j = 0; j < propertyList.arraySize; j++)
                    {
                        var itemProperty        = propertyList.GetArrayElementAtIndex(j);
                        var displayNameProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.displayName));

                        var overrideBaseValueProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.overrideBaseValue));
                        menu.AddItem(new GUIContent(displayNameProperty.stringValue),
                            overrideBaseValueProperty.boolValue, ToggleOverrideBaseValueProperty, itemProperty);
                        if (!overrideBaseValueProperty.boolValue) continue;
                        if (guiMethod == null)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(
                                $"{displayNameProperty.stringValue}");
                            GUILayout.FlexibleSpace();
                            ShowRemoveOverrideButton(itemProperty);
                            GUILayout.EndHorizontal();

                            //EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("baseValue"));
                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("targetValue"),
                                new GUIContent("New Value"));
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            guiMethod(itemProperty);
                        }
                    }

                    ShowDropDownButton(menu, $"Choose {heading} Overrides");
                    GUILayout.EndVertical();
                }
            }
        }
        
        void ShowDropDownButton(GenericMenu menu, string buttonLabel)
        {
            var buttonText = new GUIContent(buttonLabel);
            var buttonRect = GUILayoutUtility.GetRect(buttonText, "button");
            if (GUI.Button(buttonRect, buttonText))
            {
                menu.DropDown(buttonRect);
            }
        }

        void ToggleOverrideBaseValueProperty(object property)
        {
            var p                         = property as SerializedProperty;
            var overrideBaseValueProperty = p.FindPropertyRelative("overrideBaseValue");
            overrideBaseValueProperty.boolValue = !overrideBaseValueProperty.boolValue;
            p.serializedObject.ApplyModifiedProperties();
        }

        void MakeTextureReadable(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            AssetImporter.GetAtPath(path);
            var importer = (TextureImporter) TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            UpdateSampledColors();
        }
    }
}
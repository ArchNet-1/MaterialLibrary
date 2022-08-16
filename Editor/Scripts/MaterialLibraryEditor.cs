using UnityEditor;
using UnityEngine;


/// <summary>
/// Description : Material Library Editor
/// @author : Louis PAKEL
/// </summary>
namespace ArchNet.Library.Material
{
     [CustomEditor(typeof(MaterialLibrary))]
    public class MaterialLibraryEditor : Editor
    {
        int _listCount;
        int _lastDefaultValue;
        int _indexToDelete = -1;
        string[] _enumValues;
        bool _forceUpdate;

        SerializedProperty _materialList;
        SerializedProperty _enumPath;
        SerializedProperty _keyType;
        SerializedProperty _expandedSettings;
        SerializedProperty _forceDefaultValue;
        SerializedProperty _defaultValue;

        MaterialLibrary manager;

        GUIStyle _warningInfos;

        private void OnEnable()
        {
            _warningInfos = new GUIStyle();
            _warningInfos.normal.textColor = Color.red;
            _warningInfos.fontStyle = FontStyle.Bold;

            manager = target as MaterialLibrary;
        }

        private void OnDisable()
        {
            manager = null;
            _warningInfos = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _materialList = serializedObject.FindProperty("_materialList");
            _enumPath = serializedObject.FindProperty("_enumPath");
            _keyType = serializedObject.FindProperty("_keyType");
            _expandedSettings = serializedObject.FindProperty("_expandedSettings");
            _forceDefaultValue = serializedObject.FindProperty("_forceDefaultValue");
            _defaultValue = serializedObject.FindProperty("_defaultValue");

            // Key Type
            EditorGUILayout.LabelField("Key Type", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_keyType, new GUIContent(""));

            EditorGUILayout.Space(10);

            if (manager.GetKeyType() > 0 && false == manager.isKeyTypeUpToDate(_keyType.intValue))
            {
                if (EditorUtility.DisplayDialog("Warning", "This will clear set properties?\nDo you want to continue?", "Yes", "No"))
                {
                    ResetEnumPath();
                    ResetMaterialList();
                }
                else
                {
                    _keyType.intValue = manager.GetKeyType();
                }
            }
            _keyType.serializedObject.ApplyModifiedProperties();

            // Enum key type
            if (_keyType.intValue == 1)
            {
                EditorGUILayout.LabelField("Full Namespace Enum Path", EditorStyles.boldLabel);

                if (_enumPath.serializedObject.hasModifiedProperties)
                {
                    _enumPath.serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.PropertyField(_enumPath, new GUIContent(""));

                EditorGUILayout.Space(10);
            }

            // Settings
            _expandedSettings.isExpanded = EditorGUILayout.Foldout(_expandedSettings.isExpanded, "Settings");
            if (_expandedSettings.isExpanded)
            {
                // Force Default Value
                _forceDefaultValue.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Force Default Value"), _forceDefaultValue.boolValue);

                _lastDefaultValue = _defaultValue.intValue;
                if (_forceDefaultValue.boolValue)
                {
                    if (_keyType.intValue == 1)
                    {
                        if (_enumValues != null && _enumValues.Length > 0)
                        {
                            _defaultValue.intValue = EditorGUILayout.Popup(_defaultValue.intValue, _enumValues);
                            if (_defaultValue.serializedObject.hasModifiedProperties)
                            {
                                SetMaterialListDefaultValue();
                                _forceUpdate = true;
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No enum values found", _warningInfos);
                        }
                    }
                    if (_keyType.intValue == 2)
                    {
                        EditorGUILayout.PropertyField(_defaultValue, new GUIContent(""));
                        if (_defaultValue.serializedObject.hasModifiedProperties)
                        {
                            SetMaterialListDefaultValue();
                            _forceUpdate = true;
                        }
                    }
                }
                else if (_forceDefaultValue.serializedObject.hasModifiedProperties)
                {
                    _defaultValue.intValue = 0;
                    SetMaterialListDefaultValue();
                    _forceUpdate = true;
                }
            }

            EditorGUILayout.Space(10);

            // Add / Remove Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add a Material"))
            {
                if (IsConditionsOK())
                {
                    manager.AddMaterial();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Material Library", EditorStyles.boldLabel);

            if (IsConditionsOK())
            {
                HandleDatasDisplay();
                SaveMaterialListIfNecessary();
            }
            else
            {
                EditorGUILayout.LabelField("Missing parameters!", _warningInfos);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetMaterialListDefaultValue()
        {
            SerializedProperty materialData;
            SerializedProperty Key;

            _listCount = _materialList.arraySize;
            for (int i = 0; i < _listCount; i++)
            {
                materialData = _materialList.GetArrayElementAtIndex(i);
                Key = materialData.FindPropertyRelative("MaterialKey");

                if (Key.intValue == _lastDefaultValue)
                {
                    Key.intValue = _defaultValue.intValue;
                }
            }
        }

        private void HandleDatasDisplay()
        {
            SerializedProperty materialData;
            SerializedProperty materialValue;
            SerializedProperty materialKey;

            _listCount = _materialList.arraySize;
            for (int i = 0; i < _listCount; i++)
            {
                materialData = _materialList.GetArrayElementAtIndex(i);

                materialValue = materialData.FindPropertyRelative("MaterialValue");
                materialKey = materialData.FindPropertyRelative("MaterialKey");

                materialData.isExpanded = EditorGUILayout.Foldout(materialData.isExpanded, new GUIContent("Material " + i));
                if (materialData.isExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(materialValue, new GUIContent(""));
                    switch (_keyType.intValue)
                    {
                        // NONE
                        case 0:
                            break;

                        // ENUM
                        case 1:
                            materialKey.intValue = EditorGUILayout.Popup(materialKey.intValue, _enumValues, GUILayout.MaxWidth(200));
                            break;

                        // INT
                        case 2:
                            EditorGUILayout.LabelField("=>", GUILayout.MaxWidth(20));
                            EditorGUILayout.PropertyField(materialKey, new GUIContent(""), GUILayout.MaxWidth(200));
                            break;
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Are you sure to delete this material?", "Yes", "No"))
                        {
                            materialData.isExpanded = false;
                            _materialList.DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5);
                }
            }
        }

        private void ResetEnumPath()
        {
            _enumPath.stringValue = "";
            _enumPath.serializedObject.ApplyModifiedProperties();
        }

        private void ResetMaterialList()
        {
            SerializedProperty materialData;
            _listCount = _materialList.arraySize;
            for (int i = 0; i < _listCount; i++)
            {
                materialData = _materialList.GetArrayElementAtIndex(i);
                materialData.isExpanded = false;
            }
            _materialList.ClearArray();
            _enumPath.serializedObject.ApplyModifiedProperties();
        }

        private bool IsConditionsOK()
        {
            if (_keyType.intValue == 0)
            {
                return false;
            }

            if (_keyType.intValue == 1)
            {
                if (string.IsNullOrEmpty(_enumPath.stringValue))
                {
                    return false;
                }

                _enumValues = manager.GetEnumValues(_enumPath.stringValue);
                if (_enumValues == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveMaterialListIfNecessary()
        {
            if ((_materialList.serializedObject.hasModifiedProperties || _forceUpdate) && _materialList.arraySize > 0)
            {
                manager.SaveDictionnary();
                _forceUpdate = false;
            }
            _materialList.serializedObject.ApplyModifiedProperties();
        }
    }
}

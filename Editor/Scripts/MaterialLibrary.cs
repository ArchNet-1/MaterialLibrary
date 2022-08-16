using System;
using UnityEngine;
using System.Collections.Generic;
using MyMaterial = UnityEngine.Material;

/// <summary>
/// Description : Material Library is a link between Material and enum or integer
/// Avoid mass refacto on a change  on a Material
/// @author : Louis PAKEL
/// </summary>
namespace ArchNet.Library.Material
{
    // Dictionnary can't be serialized
    // We use this class as Dictionnary copy
    [Serializable]
    public class MaterialData
    {
        public int MaterialKey;
        public MyMaterial MaterialValue;
        
        public MaterialData(int materialKey, MyMaterial materialValue)
        {
            MaterialKey = materialKey;
            MaterialValue = materialValue;
        }
    }

    [CreateAssetMenu(fileName = "MaterialLibrary", menuName = "ArchNet/MaterialLibrary")]
    public class MaterialLibrary : ScriptableObject
    {
        public enum KeyType
        {
            NONE,
            ENUM,
            INT
        }

        #region SerializeField

        [SerializeField] private string _enumPath;
        [SerializeField] private List<MaterialData> _materialList;
        [SerializeField] bool _expandedSettings = true;
        [SerializeField] bool _forceDefaultValue = false;
        [SerializeField] private int _defaultValue = 0;
        [SerializeField] KeyType _keyType = KeyType.NONE;

        #endregion

        #region Private Properties

        // Our final Dictionnary
        private Dictionary<int, MyMaterial> _materialDict;
        private string[] _enumValues;
        private Type _enumType;

        #endregion

        #region Public Methods

        public MyMaterial GetMaterial(int pKeyValue)
        {
            if (CheckExistingMaterial(pKeyValue))
            {
                return _materialDict[pKeyValue];
            }

            //TODO: RETURN ERROR
            return null;
        }

        public MyMaterial GetMaterial(Enum pEnumValue)
        {
            Type actualEnumType = GetEnumType(_enumPath);

            if (_keyType != KeyType.ENUM)
            {
                Debug.LogWarning("[" + name + "] It seems that you're trying to get a Material with an enum within a library not set for enum key");
            }
            else if (pEnumValue.GetType() != actualEnumType)
            {
                Debug.LogWarning("[" + name + "] It seems that you're trying to get a Material with a different enum that the one defined to be the key in the library");
            }

            int value = Convert.ToInt32(pEnumValue);
            if (CheckExistingMaterial(value))
            {
                return _materialDict[value];
            }

            // TODO: RETURN ERROR
            return null;
        }

        #endregion

        #region Private Methods

        private bool CheckExistingMaterial(int enumValue)
        {
            if (_materialDict == null)
            {
                if (_materialList != null)
                {
                    SaveDictionnary();
                }

                if (_materialList.Count == 0)
                {
                    Debug.LogWarning("Material Library \'" + name + "\' doesn't contain anything");
                    return false;
                }
            }
            if (false == _materialDict.ContainsKey(enumValue))
            {
                string[] lEnumValues = GetEnumValues(_enumPath);
                Debug.LogWarning("Library do not contain a Material for value: " + enumValue);
                return false;
            }

            if (_materialDict.ContainsKey(enumValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Editor Methods

        /// <summary>
        /// FOR CUSTOM EDITOR PURPOSE ONLY! DO NOT USE
        /// </summary>
        public string[] GetEnumValues(string enumName)
        {
            Type type = GetEnumType(enumName);
            if (type != null)
            {
                _enumType = type;
                _enumPath = enumName;
                _enumValues = Enum.GetNames(type);
                return _enumValues;
            }

            return null;
        }

        /// <summary>
        /// FOR CUSTOM EDITOR PURPOSE ONLY! DO NOT USE
        /// </summary>
        public Type GetEnumType(string enumName)
        {
            if (false == string.IsNullOrEmpty(enumName))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType(enumName);
                    if (type == null)
                        continue;
                    if (type.IsEnum)
                        return type;
                }
            }
            return null;
        }

        /// <summary>
        /// FOR CUSTOM EDITOR PURPOSE ONLY! DO NOT USE
        /// </summary>
        public int GetKeyType()
        {
            return (int)_keyType;
        }

        /// <summary>
        /// FOR CUSTOM EDITOR PURPOSE ONLY! DO NOT USE
        /// </summary>
        public bool isKeyTypeUpToDate(int pKeyValue)
        {
            if ((int)_keyType == pKeyValue)
            { return true; }
            else
            { return false; }
        }

        /// <summary>
        /// FOR CUSTOM EDITOR PURPOSE ONLY! DO NOT USE
        /// </summary>
        public void SaveDictionnary()
        {
            if (_materialList == null)
            { return; }

            if (_materialList.Count == 0)
            {
                Debug.LogWarning("The Material List is empty");
            }
            _materialDict = new Dictionary<int, MyMaterial>();
            for (int i = 0; i < _materialList.Count; i++)
            {
                if (_materialList[i].MaterialKey != _defaultValue)
                {
                    // Check that there's not two Material for the same enum
                    if (_materialDict.ContainsKey(_materialList[i].MaterialKey))
                    {
                        if (_materialDict[_materialList[i].MaterialKey] != _materialList[i].MaterialValue)
                        {
                            if (_keyType == KeyType.ENUM)
                            {
                                Debug.LogWarning("Material \'" + _materialDict[_materialList[i].MaterialKey] + "\' is already defined for \'" + _enumValues[_materialList[i].MaterialKey] + "\'\nCannot define \'" + _materialList[i].MaterialValue + "\' as well.");
                            }
                            else
                            {
                                Debug.LogWarning("Material \'" + _materialDict[_materialList[i].MaterialKey] + "\' is already defined for \'" + _materialList[i].MaterialKey + "\'\nCannot define \'" + _materialList[i].MaterialValue + "\' as well.");
                            }
                        }
                    }
                    else
                    {
                        _materialDict.Add(_materialList[i].MaterialKey, _materialList[i].MaterialValue);
                    }
                }
            }
        }
        
        public void AddMaterial()
        {
            _materialList.Add(new MaterialData(_defaultValue, null));
        }
        
        #endregion
    }
}
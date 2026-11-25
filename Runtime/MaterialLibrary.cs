// ArchNet · Material Library
// Stores materials under a key (an enum value such as Surface.Gold, or an int), so code
// swaps materials by name instead of holding references everywhere.
// Create one with  Assets > Create > ArchNet > Material Library.
// Docs: https://archnet-plateform.fr/#MaterialLibrary

using System;
using UnityEngine;

namespace ArchNet.Library
{
    [CreateAssetMenu(fileName = "NewMaterialLibrary", menuName = "ArchNet/Material Library")]
    public class MaterialLibrary : KeyedLibrary<Material>
    {
        public Material GetMaterial(int key) => Get(key);
        public Material GetMaterial(Enum key) => Get(key);
    }
}

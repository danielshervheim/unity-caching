using UnityEditor;
using UnityEngine;

using static DSS.CoreUtils.EditorUtilities.GUIUtilities;

namespace DSS.Caching
{

[CustomEditor(typeof(Cache))]
public class CacheEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Section(string.Empty, () =>
        {
            Title("Cache");
        });

        serializedObject.ApplyModifiedProperties();
    }
}

}  // namespace Aisos.VFS
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Penumbra.EditorTools
{
    [InitializeOnLoad]
    static class PenumbraProjectRootGuard
    {
        static PenumbraProjectRootGuard()
        {
            EditorApplication.delayCall += CheckProjectRoot;
        }

        static void CheckProjectRoot()
        {
            if (!IsNestedDuplicateProjectRoot())
            {
                return;
            }

            string nestedRoot = Directory.GetParent(Application.dataPath)?.FullName;
            string correctRoot = Directory.GetParent(nestedRoot)?.FullName;

            EditorUtility.DisplayDialog(
                "Penumbra: Wrong Project Folder",
                "Unity opened a nested duplicate project folder:\n\n" +
                $"{nestedRoot}\n\n" +
                "Close Unity Hub / Unity and open this folder instead:\n\n" +
                $"{correctRoot}\n\n" +
                "The correct folder is the one that directly contains Assets, Packages, and ProjectSettings.\n" +
                "Do not open a Penumbra folder inside another Penumbra folder.",
                "OK");
        }

        static bool IsNestedDuplicateProjectRoot()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                return false;
            }

            if (!string.Equals(Path.GetFileName(projectRoot), "Penumbra", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string parent = Directory.GetParent(projectRoot)?.FullName;
            if (string.IsNullOrEmpty(parent))
            {
                return false;
            }

            return string.Equals(Path.GetFileName(parent), "Penumbra", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif

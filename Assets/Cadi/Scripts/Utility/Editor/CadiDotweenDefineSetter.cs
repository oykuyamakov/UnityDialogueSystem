#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Cadi.Scripts.Utility.Editor
{
    [InitializeOnLoad]
    public static class CadiDotweenDefineSetter
    {
        const string DEFINE = "CADI_DOTWEEN";

        static CadiDotweenDefineSetter()
        {
            bool dotweenExists = System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(asm => asm.GetType("DG.Tweening.DOTween") != null);

            var target = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);

            string current = PlayerSettings.GetScriptingDefineSymbols(target);
            var defines = current.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            bool hasDefine = defines.Contains(DEFINE);

            if (dotweenExists && !hasDefine)
            {
                defines.Add(DEFINE);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
                UnityEngine.Debug.Log($"[Cadi] DOTween detected. Added {DEFINE}.");
            }
            else if (!dotweenExists && hasDefine)
            {
                defines.Remove(DEFINE);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
                UnityEngine.Debug.Log($"[Cadi] DOTween not found. Removed {DEFINE}.");
            }
        }
    }
}
#endif
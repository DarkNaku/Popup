using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DarkNaku.Popup {
    [InitializeOnLoad]
    public static class PackageImporter {
        static PackageImporter() {
            var define = "DARKNAKU_POPUP";

            System.Array buildTargets = System.Enum.GetValues(typeof(BuildTarget));

            foreach (BuildTarget target in buildTargets) {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);

                if (BuildPipeline.IsBuildTargetSupported(buildTargetGroup, target) == false) continue;

#if UNITY_2023_1_OR_NEWER
                var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif

                if (defines.IndexOf(define) > 0) continue;

#if UNITY_2023_1_OR_NEWER
			    PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, $"{defines};{define}".Replace(";;", ";"));
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, $"{defines};{define}".Replace(";;", ";"));
#endif
            }
        }
    }
}
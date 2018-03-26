using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class BuildAssetBundle : UnityEditor.Editor
    {
        /// <summary>
        /// 点击后，所有设置了AssetBundle名称的资源会被 分单个打包出来
        /// </summary>
        [MenuItem("Tools/Build/Build (Single)")]
        static void Build_AssetBundle()
        {
            BuildPipeline.BuildAssetBundles(Application.dataPath + "/Test_AssetBundle", BuildAssetBundleOptions.None, CurrentBuildTarget());
            //刷新
            AssetDatabase.Refresh();
        }


        [MenuItem("Tools/Build/BuildAllBundle", false, 100)]
        static void BuildAllAssetBundle()
        {
            BuildModelAssetBundle.Build("Assets/Res/Model/Prefab", "Assets/StreamingAssets/Model/Prefab", CurrentBuildTarget());
            BuildUIAssetBundle.Build("Assets/Res/Prefab", "Assets/StreamingAssets/ui", CurrentBuildTarget());
        }
        

        public static BuildTarget CurrentBuildTarget()
        {
#if UNITY_STANDALONE_WIN
            BuildTarget target = BuildTarget.StandaloneWindows64;
#elif UNITY_ANDROID
        BuildTarget target = BuildTarget.Android;
#elif UNITY_WP8
#else
       BuildTarget target =  BuildTarget.iOS;
#endif
            return target;
        }
    }
}

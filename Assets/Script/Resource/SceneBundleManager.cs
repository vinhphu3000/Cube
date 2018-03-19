using CloverGame.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CloverGame.Resource
{
    public class ModelInfoData : IInfoData
    { 
        public ModelInfoData(string path, string name)
        {
            this.path = path;
            this.name = name;
        }

        public string path;
        public string name;

        public string GetBundleName()
        {
            return name;
        }

    }

    public class SceneBundleManager : IResourceManager
    {
        public static string PathModelPrefab = "/Model/Prefab";
        public static string m_bundleExt = ".assetbundle";

        public override IEnumerator LoadResource(IInfoData iData, LoadBundleFinish delFinish, object param1, object param2)
        {
            ModelInfoData infoData = iData as ModelInfoData;

            if (infoData == null)
                yield break;

#if UNITY_EDITOR
            if (PlayerSetting.SimulateAssetBundleInEditor)
            {                
                if (null != delFinish)
                {
                    string assetPath = GetBundleLoadUrl(PathModelPrefab, infoData.name.ToLower(), ".prefab");
                    GameObject gb = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    delFinish(infoData, gb, param1, param2);
                }

                yield break;
            }
#endif
       

            string loadPath = GetBundleLoadUrl(PathModelPrefab, infoData.name.ToLower(), m_bundleExt);

            AssetBundleCreateRequest req = GetBundleRequest(loadPath);
            yield return req;

            ProcessLoad(req.assetBundle, infoData, delFinish, param1, param2);
        }

        private static IEnumerator ProcessLoad( AssetBundle bundle, ModelInfoData infoData,
                                                LoadBundleFinish delFinish, object param1, object param2, bool isAync = true)
        {
            if (null == bundle)
            {
                yield break;
            }

            AssetBundleRequest req = bundle.LoadAssetAsync(infoData.GetBundleName(), typeof(GameObject));
            yield return req;

            GameObject retObj = req.asset as GameObject;

            if (null != delFinish) delFinish(infoData, retObj, param1, param2);
        }

        AssetBundleCreateRequest GetBundleRequest(string path)
        {
            return AssetBundle.LoadFromFileAsync(path);
        }

        public string GetBundleLoadUrl(string subFolder, string localName, string fileExt = "")
        {
            return ResourcePath.GetAssetsPath(subFolder, localName, fileExt);
        }
    }
}

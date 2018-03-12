using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CloverGame.Resource
{
    public class ModelInfoData : IInfoData
    {
        public static string PathModelPrefab = "/Model";
        public static string m_bundleExt = ".assetbundle";

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

        public string GetBundleLoadUrl()
        {
            return ResourcePath.GetAssetsPath(PathModelPrefab, name.ToLower(), m_bundleExt);
        }
    }

    public class SceneBundleManager : IResourceManager
    {
        public override IEnumerator LoadResource(IInfoData iData, LoadBundleFinish delFinish, object param1, object param2)
        {
            ModelInfoData infoData = iData as ModelInfoData;
            
            if (infoData == null)
                yield break;            

            string loadPath = infoData.GetBundleLoadUrl();

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
     
    }
}

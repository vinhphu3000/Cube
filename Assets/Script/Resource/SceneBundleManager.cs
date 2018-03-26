using CloverGame.Config;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

    public class BundleLoadedCache
    {
        public int refCount = 0;
        public AssetBundle cachedAssetBundle = null;
        public GameObject modelOrEffect = null;
    }

    public class SceneBundleManager : IResourceManager
    {
        public static string PathModelPrefab = "/Model/Prefab";
        public static string m_bundleExt = ".assetbundle";

        private static Dictionary<string, BundleLoadedCache> m_dicSingleBundleCache = new Dictionary<string, BundleLoadedCache>();

        private static List<string> m_dicSingleBundleRef = new List<string>();

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

            ///这里充当加载标识的作用，表示如果当前已经在加载这个资源了，则等待直到资源加载完成
            while (m_dicSingleBundleRef.Contains(infoData.GetBundleName()))
            {
                yield return 0;
            }

            ///添加对应的引用计数
            if (!m_dicSingleBundleRef.Contains(infoData.GetBundleName()))
            {
                m_dicSingleBundleRef.Add(infoData.GetBundleName());
            }

            if (LoadFromCache(infoData, delFinish, param1, param2))
            {
                yield break;
            }

            string assetBundleName = infoData.name.ToLower();

            string loadPath = GetBundleLoadUrl(PathModelPrefab, assetBundleName, m_bundleExt);

            AssetBundleCreateRequest req = GetBundleRequest(loadPath);
            yield return req;

            yield return ProcessLoad(req.assetBundle, infoData, delFinish, param1, param2);
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

            if (m_dicSingleBundleRef.Contains(infoData.GetBundleName()))
            {
                m_dicSingleBundleRef.Remove(infoData.GetBundleName());
            }

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

        /// <summary>
        /// 增加引用计数
        /// </summary>
        /// <param name="key"></param>
        public static void IncreaseBundleAssetsRef(string key)
        {
            if (m_dicSingleBundleCache.ContainsKey(key))
            {
                m_dicSingleBundleCache[key].refCount++;
            }
            else
            {
#if UNITY_EDITOR
                if (!PlayerSetting.SimulateAssetBundleInEditor)
                {
                    Debug.LogError("IncreaseBundleAssetsRef Is not Exist The uiGroupName Name is" + key); 
                }
#endif
            }
        }

        /// <summary>
        /// 减小引用计数
        /// </summary>
        /// <param name="key"></param>
        public static void DecreaseBundleAssetsRef(string key)
        {
            if (m_dicSingleBundleCache.ContainsKey(key))
            {
                if (--m_dicSingleBundleCache[key].refCount <= 0 && m_dicSingleBundleCache[key].cachedAssetBundle != null)
                {
                    ReleaseCachedBundle(key);
                }
            }
            else
            {
#if UNITY_EDITOR
                if (!PlayerSetting.SimulateAssetBundleInEditor)
                {
                    Debug.LogError("DecreaseBundleAssetsRef Is not Exist The  Name is" + key);
                }
#endif
            }
        }

        public static void ReleaseCachedBundle(string bundleName)
        {
            if (m_dicSingleBundleCache.ContainsKey(bundleName))
            {
                if (!m_dicSingleBundleRef.Contains(bundleName)) //卸载bundle 保证不在加载队列中
                {
                    AssetBundle bundle = m_dicSingleBundleCache[bundleName].cachedAssetBundle;
                    if (bundle != null)
                    {
                        bundle.Unload(true);
                    }
                    m_dicSingleBundleCache.Remove(bundleName);
                }
                else
                {
                    if (m_dicSingleBundleCache[bundleName].refCount < 0)
                    {
                        m_dicSingleBundleCache[bundleName].refCount = 0;
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                if (!PlayerSetting.SimulateAssetBundleInEditor)
                {
                    Debug.LogError("ReleaseCachedBundle Is Not Exist The AssetbundleName:" + bundleName);
                }
#endif
            }
        }

        private static bool LoadFromCache(ModelInfoData infoData, LoadBundleFinish delFinish, object param1, object param2)
        {
            string assetBundleName = infoData.GetBundleName();

            if (m_dicSingleBundleCache.ContainsKey(assetBundleName))
            {
                ///添加对应的引用计数
              //  if (!m_dicSingleBundleRef.Contains(assetBundleName))
                {
               //     m_dicSingleBundleRef.Add(assetBundleName);
                }

                GameObject modelObject = m_dicSingleBundleCache[assetBundleName].modelOrEffect;

                if (null != delFinish) delFinish(infoData, modelObject, param1, param2);
                
                return true;
            }            
            return false;
        }
    }
}

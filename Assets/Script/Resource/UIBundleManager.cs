using CloverGame.Config;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using CloverGame.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloverGame.Resource
{
    public class UIBundleCache
    {
        private int UIBundleRefCount = 0;
        public int GetRefCount
        {
            get { return UIBundleRefCount; }
        }

        public AssetBundle UICachedAssetBundle = null;
        public List<string> dependencyList = null;

        public static UIBundleCache New()
        {
            return new UIBundleCache();
        }

        //创建并且添加到Cache中
        public static UIBundleCache NewAndAddUIBundleInCache(AssetBundle CachedAssetBundle, string bundleName, List<string> dependency)
        {
            if (CachedAssetBundle == null || !string.IsNullOrEmpty(bundleName))
            {
                UIBundleCache BundleCache = New();
                BundleCache.UICachedAssetBundle = CachedAssetBundle;
                BundleCache.IncreaseUIBundleAssetsRef();
                BundleCache.dependencyList = dependency;
                return BundleCache;
            }
            else
            {
#if UNITY_EDITOR
                if (!PlayerSetting.SimulateAssetBundleInEditor)
                {
                    Debug.LogError("NewAndAddUIBundleInCache bundle is null" + bundleName);
                }
#endif          
                return null;
            }
        }

        public void IncreaseUIBundleAssetsRef()
        {

            if (UIBundleRefCount >= (int.MaxValue - 1))
            {
                //一个保护机制 例如item等 计数加和减对应不上  
                //基本不会超过上限
                return;
            }
            UIBundleRefCount++;
        }

        public void DecreaseUIBundleAssetsRef(string Key)
        {
            Key = Key.ToLower();
            UIBundleRefCount--;    
        }

        public void Unload()
        {
            if (UICachedAssetBundle != null)
            {
                UICachedAssetBundle.Unload(true);
                UICachedAssetBundle = null;
            }
        }
    }

    public class UIBundleManager : IResourceManager
    {
        public static string PathUIPrefab = "/ui/prefab";
        public static string m_bundleExt = ".assetbundle";

        public AssetBundleManifest manifest = null;

        /// <summary>
        /// 当前正在用的UI Bundle 
        /// </summary>
        private static Dictionary<string, UIBundleCache> m_UIBundleCache = new Dictionary<string, UIBundleCache>();

        private static List<string> m_BundleUILoadingList = new List<string>();

        public override IEnumerator LoadResource(IInfoData iData, LoadBundleFinish delFinish, object param1, object param2)
        {
            UIInfoData infoData = iData as UIInfoData;

            if (infoData == null)
                yield break;

#if UNITY_EDITOR
            if (PlayerSetting.SimulateAssetBundleInEditor)
            {
                string assertMainPath = GetBundleLoadUrl(infoData.path, infoData.name + ".prefab");
                GameObject objectRet = AssetDatabase.LoadAssetAtPath<GameObject>(assertMainPath);

                if (objectRet == null)
                {
                    Debug.Log(infoData.name);
                }
                delFinish(infoData, objectRet, param1, param2);
                yield break;
            }
#endif
            yield return LoadManifest();
            yield return LoadCommonUI();

            string bundleName = infoData.GetBundleName().ToLower();

            yield return LoadPrefabUI(bundleName, true, true);

            yield return LoadGameObject(bundleName, infoData, delFinish, param1, param2);

            yield return null;
        }

        IEnumerator LoadCommonUI()
        {
            yield return null;
        }

        public string GetBundleLoadUrl(string subFolder, string localName, string fileExt = "")
        {
            return ResourcePath.GetAssetsPath(subFolder, localName, fileExt);
        }

        public IEnumerator LoadGameObject(string bundleName, UIInfoData infoData, 
                                          LoadBundleFinish delFinish, object param1, object param2, bool isAync = true)
        {
            GameObject retObj = null;

            if (m_UIBundleCache.ContainsKey(bundleName))
            {
                AssetBundle bundle = m_UIBundleCache[bundleName].UICachedAssetBundle;
                if (isAync)
                {
                    AssetBundleRequest req = bundle.LoadAssetAsync(infoData.name, typeof(GameObject));
                    yield return req;
                    retObj = req.asset as GameObject;
                }
                else
                {
                    retObj = bundle.LoadAsset(infoData.name, typeof(GameObject)) as GameObject;
                }

                if (null != delFinish) delFinish(infoData, retObj, param1, param2);
            }
        }

        /// <summary>
        /// 加载UIBundle
        /// </summary>
        /// <param name="bundleName">bundle的名字</param>
        /// <param name="needDependency">是否是AssetBundle的依赖加载</param>
        /// <param name="isAync">是否是异步加载 true为异步加载，false为同步加载</param>
        /// <returns></returns>
        public IEnumerator LoadPrefabUI(string bundleName, bool needDependency, bool isAync = true)
        {
            //保证bundle不会被同时加载
            while (m_BundleUILoadingList.Contains(bundleName))
            {
                yield return null;
            }

            //因同时加载卡在协程的继续走会发现已经加载好，直接返回
            if (m_UIBundleCache.ContainsKey(bundleName))
            {
                m_UIBundleCache[bundleName].IncreaseUIBundleAssetsRef();
                string[] dependencies = manifest.GetAllDependencies(Utils.Utils.Text.Format("prefab/{0}.assetbundle", bundleName));
                for (int i = 0; i < dependencies.Length; ++i)
                {
                    string dependencyPath = dependencies[i];
                    if (!dependencyPath.Contains("commondata"))
                    {
                        string name = dependencyPath.Substring(0, dependencyPath.Length - 12);
                        name = name.Substring(7);
                        UIBundleCache cache = null;
                        m_UIBundleCache.TryGetValue(name, out cache);
                        if (cache != null)
                            cache.IncreaseUIBundleAssetsRef();
                        else
                            Debug.LogWarning("Add RefCount failed: " + name);
                    }                       
                }
                yield break;
            }

            m_BundleUILoadingList.Add(bundleName);

            List<string> list = null;
            if (needDependency)
            {
                //先判断是否有依赖项，有则加载之
                string[] dependencies = manifest.GetAllDependencies(CloverGame.Utils.Utils.Text.Format("prefab/{0}.assetbundle", bundleName));
                for (int i = 0; i < dependencies.Length; ++i)
                {
                    string dependencyPath = dependencies[i];
                    if (!dependencyPath.Contains("commondata"))
                    {
                        if (list == null)
                            list = new List<string>();
                        string name = dependencyPath.Substring(0, dependencyPath.Length - 12);
                        name = name.Substring(7);
                        list.Add(name);
                        yield return LoadPrefabUI(name, false);
                    }                        
                }
            }

            string loadPath = GetBundleLoadUrl(PathUIPrefab, bundleName.ToLower(), m_bundleExt);

            AssetBundle loadedBundle = null;
            if (isAync)
            {
                AssetBundleCreateRequest req = GetBundleRequest(loadPath);
                yield return req;
                loadedBundle = req.assetBundle;
            }
            else
            {
                loadedBundle = AssetBundle.LoadFromFile(loadPath);
            }

            if (loadedBundle != null)
            {
                if (!m_UIBundleCache.ContainsKey(bundleName))
                {
                    UIBundleCache bundleCache = UIBundleCache.NewAndAddUIBundleInCache(loadedBundle, bundleName, list);
                    if (bundleCache != null)
                    {
                        m_UIBundleCache.Add(bundleName, bundleCache);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    if (!PlayerSetting.SimulateAssetBundleInEditor)
                    {
                        Debug.LogError("Load UIBundle Is Not Exist The Bundle Name is" + bundleName);
                    }
#endif
                }
                m_BundleUILoadingList.Remove(bundleName);
            }
        }

        IEnumerator LoadManifest()
        {
#if UNITY_EDITOR
            if (PlayerSetting.SimulateAssetBundleInEditor)
                yield break;
#endif
            if (manifest != null)
            {
                yield break;
            }

            AssetBundleCreateRequest createReq = AssetBundle.LoadFromFileAsync(GetBundleLoadUrl("/ui", "ui"));
            yield return createReq;
            if (createReq != null && createReq.assetBundle != null)
            {
                manifest = createReq.assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
            }
        }

        AssetBundleCreateRequest GetBundleRequest(string path)
        {
            return AssetBundle.LoadFromFileAsync(path);
        }

        public void ReleaseOneUIBundle(string bundleName)
        {
            bundleName = bundleName.ToLower();
            UIBundleCache cache = null;
            m_UIBundleCache.TryGetValue(bundleName, out cache);
            if (cache != null)
            {
                if (cache.dependencyList != null)
                {
                    foreach (var v in cache.dependencyList)
                    {
                        ReleaseOneUIBundle(v);
                    }
                }
                cache.DecreaseUIBundleAssetsRef(bundleName);

                if(cache.GetRefCount <= 0  && !m_BundleUILoadingList.Contains(bundleName))
                {
                    cache.Unload();

                    if (m_UIBundleCache.ContainsKey(bundleName))
                    {
                        m_UIBundleCache.Remove(bundleName);
                    }
                    else
                    {
#if UNITY_EDITOR
                        if (!PlayerSetting.SimulateAssetBundleInEditor)
                        {
                            Debug.LogError("DecreaseUIBundleAssetsRef Is not Exist The uiGroupName Name is" + bundleName);
                        }
#endif
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                if (!PlayerSetting.SimulateAssetBundleInEditor)
                { 
                    Debug.LogError("Unload  bundle Is not Exist The  Name is" + bundleName);
                }
#endif
            }
        }

        public IEnumerator OnUIDestroy(string uiName)
        {
            yield return 0;

            UIInfoData infoData = UIInfoData.GetUIInfoDataByPrefabStr(uiName);
            if(infoData == null)
            {
                yield break;
            }

            ReleaseOneUIBundle(infoData.GetBundleName());
        }
    }
}

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using CloverGame.Manager;
using System.Collections;
using CloverGame.Config;
using CloverGame.Utils;

namespace CloverGame.Resource
{
    public class BundleManager : Singleton<BundleManager>
    {
        private UIBundleManager     uiBundleManager;
        private SceneBundleManager  sceneBundleManager;
        public void Init()
        {
            uiBundleManager     = new UIBundleManager();
            sceneBundleManager = new SceneBundleManager();
        }

        public IEnumerator LoadUI(  IInfoData infoData,
                                    IResourceManager.LoadBundleFinish delFinish,
                                    object param1,
                                    object param2)
        {
            yield return uiBundleManager.LoadResource(infoData, delFinish, param1, param2);
        }

        public IEnumerator OnUIDestroy(string uiName)
        {
            yield return uiBundleManager.OnUIDestroy(uiName);
        }

        public IEnumerator LoadSceneResource(   IInfoData infoData,
                                                IResourceManager.LoadBundleFinish delFinish,
                                                object param1,
                                                object param2)
        {
            yield return sceneBundleManager.LoadResource(infoData, delFinish, param1, param2);
        }

    }
}

using UnityEngine;
using CloverGame.Resource;
using CloverGame.UI;
using System.Collections.Generic;

namespace CloverGame.Manager
{
    public class UIManager : MonoBehaviour
    {
        public class UIWaitLoadCallBack
        {
            public UIWaitLoadCallBack(IResourceManager.LoadBundleFinish loadCallBack, object param)
            {
                this.loadCallBack = loadCallBack;
                this.param = param;
            }
            public IResourceManager.LoadBundleFinish loadCallBack;
            public object param;
        }

        public delegate void OnOpenUIDelegate(bool bSuccess, object param);
        public delegate void OnLoadUIItemDelegate(GameObject resItem, object param1);

        //点击showui次数缓存
        private Dictionary<string, int> m_dicWaitLoad = new Dictionary<string, int>();

        //回调函数缓存
        private Dictionary<string, UIWaitLoadCallBack> m_showUIWaitLoadCallBack = new Dictionary<string, UIWaitLoadCallBack>();

        private static UIManager m_Instance = null;

        public static UIManager Instance()
        {
            return m_Instance;
        }

        void Awake()
        {
            m_Instance = this;
            BundleManager.Instance.Init();
            UILayerManager.Instance.InitLayers(gameObject.transform);
        }

        void OnDestroy()
        {
            m_Instance = null;
            m_dicWaitLoad.Clear();
            m_showUIWaitLoadCallBack.Clear();
        }

        public bool ShowUI(UIInfoData infoData, OnOpenUIDelegate delOpenUI = null, object param = null)
        {

            //已经显示了
            if (UILayerManager.Instance.IsUIShow(infoData))
            {
                Debug.Log("ShowUI :" + infoData.name + " already showed!");
                return true;
            }

            int refCount = AddLoadDicRefCount(infoData.name);

            //已经在加载
            if (refCount > 1)
            {
                return true;
            }           

            //从缓存里面找，如果找到了，则直接进行显示
            if (UILayerManager.Instance.IsConatain(infoData))
            {
                Debug.Log("ShowUI :" +infoData.name + " already exist!");
                GameObject uiGameObject = UILayerManager.Instance.GetUI(infoData);
                DoAddUI(infoData, uiGameObject, delOpenUI, param);
                return true;
            }            

            m_Instance.LoadUI(infoData, delOpenUI, param);
            return true;
        }

        /// <summary>
        /// 根据uidata加载对应的ui资源
        /// </summary>
        void LoadUI(UIInfoData uiData, object delOpenUI = null, object param = null)
        {            
            StartCoroutine(BundleManager.Instance.LoadUI(uiData, DoAddUI, delOpenUI, param));
        }

        void DoAddUI(IInfoData iData, GameObject curWindow, object fun, object param)
        {            
            UIInfoData uiData = iData as UIInfoData;

            Debug.Log("DoAddUI:" + uiData.name);

            if (!m_dicWaitLoad.Remove(uiData.name))
            {
                DestroyBundle(uiData.name);
                return;
            }

            UILayer.ShowUIResult showUIResult = UILayerManager.Instance.ShowUI(uiData, curWindow);

            if(showUIResult != UILayer.ShowUIResult.ErrorNotShowed)
            {
                if (null != fun)
                {
                    OnOpenUIDelegate delOpenUI = fun as OnOpenUIDelegate;
                    delOpenUI(curWindow != null, param);
                }
            }
        }

        /// <summary>
        ///  关闭UI，根据类型不同，触发不同行为
        /// </summary>
        /// <param name="pathData"></param>
        public  void CloseUI(UIInfoData pathData)
        {
            if (null == m_Instance)
            {
                return;
            }

            ///删除记录的逻辑
            RemoveLoadDicRefCount(pathData.name);
            UILayerManager.Instance.CloseUI(pathData);
        }

        /// <summary>
        /// 删除ui及卸载ui使用的资源
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public void DestroyUI(string name, GameObject obj)
        {
            obj.SetActive(false);
            Destroy(obj);
            DestroyBundle(name);
        }

        public void DestroyBundle(string name)
        {
            StartCoroutine(BundleManager.Instance.OnUIDestroy(name));
        }

        public bool LoadItem(UIInfoData pathData, OnLoadUIItemDelegate delLoadItem, object param = null, bool IsLoadItem = false)
        {
            LoadUIItem(pathData, delLoadItem, param, IsLoadItem);
            return true;
        }

        void LoadUIItem(UIInfoData uiData, OnLoadUIItemDelegate delLoadItem, object param = null, bool IsLoadItem = false)
        {
            StartCoroutine(BundleManager.Instance.LoadUI(uiData, DoLoadUIItem, delLoadItem, param));
        }

        void DoLoadUIItem(IInfoData iData, GameObject curItem, object fun, object param)
        {
            UIInfoData uiData = iData as UIInfoData;

            if (null != fun)
            {
                OnLoadUIItemDelegate delLoadItem = fun as OnLoadUIItemDelegate;
                delLoadItem(curItem, param);
            }
        }

        int AddLoadDicRefCount(string pathName)
        {
            if (m_dicWaitLoad.ContainsKey(pathName))
            {
                m_dicWaitLoad[pathName]++;
            }
            else
            {
                m_dicWaitLoad.Add(pathName, 1);
            }

            return m_dicWaitLoad[pathName];
        }

        int RemoveLoadDicRefCount(string pathName)
        {
            if (!m_dicWaitLoad.ContainsKey(pathName))
            {
                return 0;
            }

            m_dicWaitLoad[pathName]--;
            if (m_dicWaitLoad[pathName] <= 0)
            {
                m_dicWaitLoad.Remove(pathName);
                return 0;
            }
            else
            {
                return m_dicWaitLoad[pathName];
            }
        }
    }
}

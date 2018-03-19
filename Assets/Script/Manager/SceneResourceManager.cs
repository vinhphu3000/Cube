using CloverGame.Resource;
using UnityEngine;
using CloverGame.Obj;

namespace CloverGame.Manager
{
    public class SceneResourceManager : MonoBehaviour
    {
        public delegate void OnLoadResourceDelegate(bool bSuccess, object param);

        private static SceneResourceManager m_Instance = null;

        public GameObject clientChaRoot;

        private GameObject NPCPlayerObjPrefab;

        public static SceneResourceManager Instance()
        {
            return m_Instance;
        }

        void Awake()
        {
            m_Instance = this;
        }

        void OnDestroy()
        {
            m_Instance = null;
        }

        public void LoadResource(ModelInfoData infoData, OnLoadResourceDelegate delOpenUI = null, object param = null)
        {
            GameObject clientObj = CreateClientChaRoot();

            if (clientObj == null) return;

            ClientCha clientCha = clientObj.GetComponent<ClientCha>();
            if (clientCha == null)
            {
                clientCha = clientObj.AddComponent<ClientCha>();
            }

            CreateNPCMdjInitProperty(clientCha, infoData);           
        }

        public void CreateNPCMdjInitProperty(ClientCha npcCha, ModelInfoData infoData, OnLoadResourceDelegate delOpenUI = null, object param = null)
        {            
            StartCoroutine(BundleManager.Instance.LoadSceneResource(infoData, DoLoadResource, delOpenUI, npcCha));
        }

        void DoLoadResource(IInfoData iData, GameObject loadObj, object fun, object param)
        {
            ModelInfoData infoData = iData as ModelInfoData;

            if (infoData == null) return;

            Debug.Log("DoLoadResource:" + infoData.name);

            ClientCha clientCha = param as ClientCha;

            if (clientCha == null) return;

            GameObject newModelObj = GameObject.Instantiate(loadObj, clientCha.transform) as GameObject;

            if (newModelObj == null)
            {
                return;
            }

        }

        /// <summary>
        /// 向内存中加载一个资源
        /// 因为只会创建内存数据，所以不会调用instantiate方法
        /// 此方法不支持异步加载
        /// 但是失败会输出日志
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
        public static UnityEngine.Object LoadResource(string resPath, System.Type systemTypeInstance = null)
        {
            UnityEngine.Object resObject = null;
            if (null == systemTypeInstance)
            {
                resObject = Resources.Load(resPath);
            }
            else
            {
                resObject = Resources.Load(resPath, systemTypeInstance);
            }

            if (null != resObject)
            {
              //  IncreaseResourceLoadCount(resPath);
            }

            return resObject;
        }

        public ClientCha CreateClientCha()
        {
            return new ClientCha();
        }

        private GameObject CreateClientChaRoot()
        {
            if (NPCPlayerObjPrefab == null)
            {
                NPCPlayerObjPrefab = LoadResource("Model/NPCRoot") as GameObject;
            }

            GameObject clientObj = Instantiate(NPCPlayerObjPrefab) as GameObject;

            if (clientObj == null) return null;

            clientObj.transform.parent = clientChaRoot.transform;

            return clientObj;
        }
    }
}

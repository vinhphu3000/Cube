using UnityEngine;
using CloverGame.UI;
using CloverGame.Resource;
using UILayerType = CloverGame.Resource.UIInfoData.UIType;
using CloverGame.Utils;

namespace CloverGame.Manager
{
    public class UILayerManager : Singleton<UILayerManager>
    {
        private UILayer[] m_UILayers = null;
        public UILayer[] AllUILayers
        {
            get { return m_UILayers; }
        }

        public UILayer pop_Layer = null;

        public void InitLayers(Transform layersParent)
        {
            m_UILayers = new UILayer[(int)UILayerType.TYPE_MAX];
            m_UILayers[(int)UILayerType.TYPE_BASE] = LayerFactory.CreateUILayer(UILayerType.TYPE_BASE, layersParent);
            m_UILayers[(int)UILayerType.TYPE_POP] = LayerFactory.CreateUILayer(UILayerType.TYPE_POP, layersParent);
            m_UILayers[(int)UILayerType.TYPE_MESSAGE] = LayerFactory.CreateUILayer(UILayerType.TYPE_MESSAGE, layersParent);
            m_UILayers[(int)UILayerType.TYPE_DEATH] = LayerFactory.CreateUILayer(UILayerType.TYPE_DEATH, layersParent);

            pop_Layer = m_UILayers[(int)UILayerType.TYPE_POP];
        }

        public UILayer.ShowUIResult ShowUI(UIInfoData uiData, GameObject curWindow)
        {
            if (null != curWindow && null != m_UILayers[(int)uiData.uiType])
            {
                return m_UILayers[(int)uiData.uiType].ShowUI(uiData, curWindow);
            }
            return UILayer.ShowUIResult.ErrorNotShowed;
        }

        public UILayer.DestroyUIResult CloseUI(UIInfoData uiData)
        {
            if (null != m_UILayers[(int)uiData.uiType])
            {
                return m_UILayers[(int)uiData.uiType].CloseUI(uiData.name);
            }

            return UILayer.DestroyUIResult.NotHaveUI;
        }

        public bool IsUIShow(UIInfoData pathData)
        {
            bool result = m_UILayers[(int)pathData.uiType].addedUIs.ContainsKey(pathData.name);
            if (result)
            {
                result = m_UILayers[(int)pathData.uiType].addedUIs[pathData.name].activeSelf;
            }
            return result;
        }

        public bool IsConatain(UIInfoData pathData)
        {
            return m_UILayers[(int)pathData.uiType].addedUIs.ContainsKey(pathData.name);
        }

        public GameObject GetUI(UIInfoData pathData)
        {
            return m_UILayers[(int)pathData.uiType].addedUIs[pathData.name];
        }
    }
}

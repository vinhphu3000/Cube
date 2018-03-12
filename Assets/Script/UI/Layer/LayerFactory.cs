using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CloverGame.Resource;

namespace CloverGame.UI
{
    public class LayerFactory
    {
        public static UILayer CreateUILayer(UIInfoData.UIType layerType, Transform root)
        {
            string name = Enum.GetName(layerType.GetType(), layerType);
            Transform layerTransform = root.FindChild(name);
            if (layerTransform == null)
            {
                GameObject obj = new GameObject();
                obj.transform.parent = root;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.name = name;
                obj.layer = (int)layerType;
                layerTransform = obj.transform;
                layerTransform.gameObject.AddComponent<UIPanel>();
            }
            layerTransform.gameObject.SetActive(true);

            UILayer uiLayer = new UILayer();
            if (uiLayer != null)
            {
                uiLayer.attachedParent = layerTransform;
                uiLayer.layerType = layerType;
                layerTransform.gameObject.layer = (int)uiLayer.layerType;
            }
            return uiLayer;
        }
    }
}

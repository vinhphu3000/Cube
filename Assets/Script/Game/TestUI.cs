using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CloverGame.Manager;
using CloverGame.Resource;

namespace CloverGame.Cube
{
    public class TestUI: MonoBehaviour
    {

        public GameObject itemParent;

        private int index = 0;

        void OnGUI()
        {

            if (GUILayout.Button("showUI"))
            {
                UIManager.Instance().ShowUI(UIInfo.CubeItem);
            }
            if (GUILayout.Button("closeUI"))
            {
                UIManager.Instance().CloseUI(UIInfo.CubeItem);
            }

            if (GUILayout.Button("show Item UI"))
            {
                UIManager.Instance().LoadItem(UIInfo.CubeItem, OnLoadUIItemDelegate);
            }

            if (GUILayout.Button("show Model"))
            {
                ModelInfoData infoData = new ModelInfoData("Model", "Ethan");

                SceneResourceManager.Instance().LoadResource(infoData);
            }    

        }

        public void OnLoadUIItemDelegate(GameObject curWindow, object param)
        {
            index++;

            GameObject newWindow = Instantiate(curWindow, Vector3.zero, Quaternion.identity) as GameObject;

            if (null != newWindow)
            {
                newWindow.transform.localScale = Vector3.one;
                newWindow.transform.parent = itemParent.transform;
                newWindow.transform.localPosition = new Vector3(index * 0.1f, index * 0.1f, index * 0.1f);
            }
        }
    }
}

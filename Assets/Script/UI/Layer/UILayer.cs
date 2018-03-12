using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CloverGame.Resource;
using CloverGame.Manager;

namespace CloverGame.UI
{
    public class UILayer
    {
        public enum ShowUIResult
        {
            /// <summary>
            /// 从没有load状态到显示UI的状态
            /// </summary>
            ShowedFromNotExist,
            /// <summary>
            /// 从隐藏状态到显示UI的状态
            /// </summary>
            ShowedFromDeactive,
            /// <summary>
            /// 当前已经显示UI了，这次调用是多余的
            /// </summary>
            ShowedDuplicate,
            /// <summary>
            /// 显示UI失败，没有显示UI
            /// </summary>
            ErrorNotShowed,
        }

        public enum DestroyUIResult
        {
            /// <summary>
            /// 删除UI，但是UI不存在
            /// </summary>
            NotHaveUI,
            /// <summary>
            /// 删除UI从True设置为False
            /// </summary>
            SetActiveFromTrue2False,
            /// <summary>
            /// 设置false原始状态就是false
            /// </summary>
            SetActiveFalseStateStay,
            /// <summary>
            /// 删除Obj
            /// </summary>
            DestroyObj,
        }

        ///当前layer上所有添加的UI包括显示的和未显示的UI
        public Dictionary<string, GameObject> addedUIs = new Dictionary<string, GameObject>();

        ///当前layer的Transform
        public Transform attachedParent;

        ///当前的layer标识
        public UIInfoData.UIType layerType;

        public virtual ShowUIResult ShowUI(UIInfoData uiData, GameObject curWindow)
        {
            ShowUIResult result = ShowUIResult.ErrorNotShowed;

            if (addedUIs.ContainsKey(uiData.name))
            {
                if (addedUIs[uiData.name].activeSelf == false)
                {
                    addedUIs[uiData.name].SetActive(true);
                    result = ShowUIResult.ShowedFromDeactive;
                }
                else
                {
                    result = ShowUIResult.ShowedDuplicate;
                }
            }
            else
            {
                GameObject newWindow = GameObject.Instantiate(curWindow, Vector3.zero, Quaternion.identity, attachedParent) as GameObject;

                if (null != newWindow)
                {
                    newWindow.transform.localScale = Vector3.one;
                    addedUIs.Add(uiData.name, newWindow);
                    result = ShowUIResult.ShowedFromNotExist;
                }
            }

            return result;
        }

        public virtual DestroyUIResult CloseUI(string uiName)
        {
            return TryDestroyUI(uiName);
        }

        public DestroyUIResult TryDestroyUI(string curName)
        {
            if (!addedUIs.ContainsKey(curName))
            {
                return DestroyUIResult.NotHaveUI;
            }

            UIManager.Instance().DestroyUI(curName, addedUIs[curName]);
            addedUIs.Remove(curName);
            return DestroyUIResult.DestroyObj;
        }
    }
}

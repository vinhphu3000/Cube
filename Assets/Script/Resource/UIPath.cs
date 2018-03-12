using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloverGame.Resource
{
    public class UIInfoData : IInfoData
    {
        public enum UIType
        {         
            TYPE_BASE,          
            TYPE_POP,          
            TYPE_MESSAGE,       
            TYPE_DEATH,         
            TYPE_MAX            
        };

        public UIInfoData(string path,string name, UIType uiType,string uiGroupName = null)
        {
            this.path           = path;
            this.name           = name;
            this.uiType         = uiType;
            this.uiGroupName    = uiGroupName;

            m_DicUIName.Add(name, this);
        }

        public string path;
        public string name;
        public UIType uiType;
        public string uiGroupName;

        public static Dictionary<string, UIInfoData> m_DicUIName = new Dictionary<string, UIInfoData>();

        public static UIInfoData GetUIInfoDataByPrefabStr(string str)
        {
            if (!m_DicUIName.ContainsKey(str))
            {
                return null;
            }
            return m_DicUIName[str];
        }

        public string GetBundleName()
        {
            return uiGroupName == null ? name : uiGroupName;
        }
    }

    public class UIInfo
    {
        public static UIInfoData CubeItem = new UIInfoData("/Prefab", "Cube", UIInfoData.UIType.TYPE_POP);
    }
}

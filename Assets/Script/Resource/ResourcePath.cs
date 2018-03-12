using CloverGame.Config;
using System.IO;
using System.Text;
using UnityEngine;

namespace CloverGame.Resource
{
    public class ResourcePath 
    {
        public static string m_BundleDataAssetsPath = "/Res";
        public static string m_AssetsPathRoot = Application.streamingAssetsPath;
        public static string m_PersistentPathRoot = Application.persistentDataPath + "/ResData";
        public static string m_loadUrlHead = "file://";
        public static string m_bundleExt = ".assetbundle";

        public static string GetAssetsPath(string subFolder, string localName, string fileExt)
        {
            StringBuilder builder = StringBuilderCache.Acquire();
#if UNITY_EDITOR
            if(PlayerSetting.SimulateAssetBundleInEditor)
            {
                builder.Append("Assets");
                builder.Append(m_BundleDataAssetsPath);
                builder.Append(subFolder);
                builder.Append("/");
                builder.Append(localName);
                builder.Append(fileExt);
                return StringBuilderCache.GetStringAndRelease(builder);
            }
#endif

#if !UNITY_ANDROID
            //builder.Append(m_loadUrlHead);
#endif
            builder.Append(m_AssetsPathRoot);
            builder.Append(subFolder);
            builder.Append("/");
            builder.Append(localName);
            builder.Append(fileExt);

            return StringBuilderCache.GetStringAndRelease(builder);
        }

        /// <summary>
        /// 持久化路径，主要是热更新路径
        /// </summary>
        /// <param name="subFolder"></param>
        /// <param name="localName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static string GetPersistentPath(string subFolder, string localName, string fileExt)
        {
            StringBuilder builder = StringBuilderCache.Acquire();
            builder.Append(m_PersistentPathRoot);
            builder.Append(subFolder);
            builder.Append("/");
            builder.Append(localName);
            builder.Append(fileExt);
            string localPath = builder.ToString();

            if (File.Exists(m_PersistentPathRoot))
            {
                builder.Insert(0, m_loadUrlHead);
                return StringBuilderCache.GetStringAndRelease(builder);
            }
            return null;
        }
    }
}

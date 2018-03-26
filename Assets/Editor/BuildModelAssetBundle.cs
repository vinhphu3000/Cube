using CloverGame.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class BuildModelAssetBundle : UnityEditor.Editor
    {

        static string[] modelFileEnd = new string[] { "prefab" };

        public static void Build(string selectPath, string outputPath, BuildTarget buildTarget)
        {
            Utils.CheckTargetPath(outputPath);

            string startPath = selectPath.Replace("\\", "/");
            Debug.Log("==startPath:" + startPath);
 
            Dictionary<string, string> singlePrefabs = GetFileDicInPath(startPath, modelFileEnd);
            Dictionary<string, Object> singleObjsDic = GetFolderObjectDicByPathGroup(singlePrefabs, selectPath);

            ///singlePrefab类型的AssetBundleBuild数组
            List<AssetBundleBuild> singleBundleBuildList = new List<AssetBundleBuild>();
            foreach (KeyValuePair<string, string> curObjPath in singlePrefabs)
            {
                string curKey = curObjPath.Key;
                if (!singleObjsDic.ContainsKey(curKey))
                {
                    Debug.LogError(curKey);
                    continue;
                }
               
                AssetBundleBuild currentBuildInfo = new AssetBundleBuild();

                string[] assetName = new string[1];
                assetName[0] = curObjPath.Value;
                currentBuildInfo.assetNames = assetName;
                currentBuildInfo.assetBundleName = curObjPath.Key + ".assetbundle";

                singleBundleBuildList.Add(currentBuildInfo);
            }

            BuildPipeline.BuildAssetBundles(outputPath, singleBundleBuildList.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
        }

        /// <summary>
        ///  返回name:path字典
        ///  返回的name为相对于path的路径不包括文件后缀
        ///  path为相对于Assets的路径，不包括Asset
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileEndArray"></param>
        /// <returns></returns>
        static Dictionary<string, string> GetFileDicInPath(string path, string[] fileEndArray)
        {
            path = path.Replace('\\', '/');
            Dictionary<string, string> retDic = new Dictionary<string, string>();
            GetAssetInPath(retDic, path, path, fileEndArray);
            return retDic;
        }

        /// <summary>
        /// 根据key和path返回key和object
        /// </summary>
        /// <param name="pathDic"></param>
        /// <param name="selectPath"></param>
        /// <returns></returns>
        static Dictionary<string, Object> GetFolderObjectDicByPathGroup(Dictionary<string, string> pathDic, string selectPath)
        {
            Dictionary<string, Object> retDic = new Dictionary<string, Object>();
            foreach (KeyValuePair<string, string> curPair in pathDic)
            {
                if (curPair.Value.Replace('\\', '/').Contains(selectPath.Replace('\\', '/')))
                {
                    retDic.Add(curPair.Key, AssetDatabase.LoadMainAssetAtPath(curPair.Value));
                }
            }
            return retDic;
        }

        static void GetAssetInPath(Dictionary<string, string> retDic, string startPath, string curPath, string[] fileEndArray)
        {
            string[] fileList = Directory.GetFiles(curPath);
            string[] dictionaryList = Directory.GetDirectories(curPath);

            foreach (string curFile in fileList)
            {
                foreach (string fileEnd in fileEndArray)
                {
                    if (curFile.EndsWith(fileEnd))
                    {
                        string curFilePath = curFile.Replace('\\', '/');
                        curFilePath = curFilePath.Substring(startPath.Length + 1);
                        //Debug.Log("Add Dic:" + curFilePath.Substring(0, curFilePath.Length - fileEnd.Length - 1));
                        retDic.Add(curFilePath.Substring(0, curFilePath.Length - fileEnd.Length - 1), curFile.Replace(Application.dataPath, "Assets").Replace('\\', '/'));
                        break;
                    }
                }
            }

            // 逐层目录开始遍历，获取所有的file end的文件
            foreach (string curDic in dictionaryList)
            {
                string curDicName = curDic.Replace('\\', '/');
                GetAssetInPath(retDic, startPath, curDicName, fileEndArray);
            }

        }
    }
}

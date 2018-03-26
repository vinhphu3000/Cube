using CloverGame.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class BuildUIAssetBundle : UnityEditor.Editor
    {
        public static string UIFontName = "font.assetbundle";
        public static string UIMainName = "main.assetbundle";
        public static string UICommonName = "common.assetbundle";

        public static void Build(string selectPath, string outputPath, BuildTarget buildTarget)
        {
            Utils.CheckTargetPath(outputPath);

            //获取路径下的prefab
            Dictionary<string, List<string>> mainUIPrefabs = GetPrefabGroupInPath(selectPath);

            //获取prefab的依赖项
            Object[] mainUIdependencies = GetDependObjectByPathList(mainUIPrefabs);

            List<string> commonDependPathList = new List<string>();
            List<string> fontDependPathList = new List<string>();
            List<string> mainUIDependPathList = new List<string>();

            //获取通用依赖项，贴图，声音，shader
            CheckCommonDependFiles(commonDependPathList, mainUIdependencies);

            //获取字体依赖
            CheckFontDependFiles(fontDependPathList, mainUIdependencies);

            //
            CheckUIDependFiles("Res/Texture/main", mainUIDependPathList, mainUIdependencies);

            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();

            //字体包
            AssetBundleBuild fontBuild = new AssetBundleBuild();
            fontBuild.assetBundleName = "CommonData/" + UIFontName;
            fontBuild.assetNames = fontDependPathList.ToArray();
            buildList.Add(fontBuild);

            //common包，公用纹理+声音+shader
            AssetBundleBuild commonBuild = new AssetBundleBuild();
            commonBuild.assetBundleName = "CommonData/" + UICommonName;
            commonBuild.assetNames = commonDependPathList.ToArray();
            buildList.Add(commonBuild);

            //main common纹理包
            AssetBundleBuild mainCommonBuild = new AssetBundleBuild();
            mainCommonBuild.assetBundleName = "CommonData/" + UIMainName;
            mainCommonBuild.assetNames = mainUIDependPathList.ToArray();
            buildList.Add(mainCommonBuild);

            //main路径下prefab打包
            foreach (KeyValuePair<string, List<string>> obj in mainUIPrefabs)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = "Prefab/" + obj.Key + ".assetbundle";
                //GetDependencyPrefabs(obj.Key, obj.Value);
                build.assetNames = obj.Value.ToArray();
                buildList.Add(build);
            }

            AssetBundleBuild[] buildMap = buildList.ToArray();
            BuildPipeline.BuildAssetBundles(outputPath, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 获取自定目录下的prefab，对于子目录按照group类型生成结果
        /// </summary>
        /// <param name="path"></param>
        /// <returns> 返回字典key为groupName，value为pathList</returns>
        static Dictionary<string, List<string>> GetPrefabGroupInPath(string path)
        {
            Dictionary<string, List<string>> retDic = new Dictionary<string, List<string>>();
            string[] fileList = Directory.GetFiles(path);
            string[] dictionaryList = Directory.GetDirectories(path);
            foreach (string curFile in fileList)
            {
                if (curFile.EndsWith("prefab"))
                {
                    List<string> curPathList = new List<string>();
                    curPathList.Add(curFile.Replace(Application.dataPath, "Assets"));
                    string curFilePath = curFile.Replace('\\', '/');
                    curFilePath = curFilePath.Substring(curFilePath.LastIndexOf('/') + 1);
                    Debug.Log("Add Dic:" + curFilePath.Substring(0, curFilePath.Length - 7));
                    retDic.Add(curFilePath.Substring(0, curFilePath.Length - 7), curPathList);
                }
            }

            foreach (string curDic in dictionaryList)
            {
                fileList = Directory.GetFiles(curDic);
                string curDicName = curDic.Replace('\\', '/');
                curDicName = curDicName.Substring(curDicName.LastIndexOf('/') + 1);
                Debug.Log("Add Dic:" + curDicName);
                List<string> curPathList = new List<string>();
                foreach (string curFile in fileList)
                {
                    if (curFile.EndsWith("prefab"))
                    {
                        curPathList.Add(curFile.Replace(Application.dataPath, "Assets"));

                        Debug.Log("============Add Dic:" + curDicName + " --path :" + curFile.Replace(Application.dataPath, "Assets"));
                    }
                }
                retDic.Add(curDicName, curPathList);
            }
            return retDic;
        }

        /// <summary>
        /// 获取所有path对应的资源的依赖项
        /// </summary>
        /// <param name="pathList"></param>
        /// <returns></returns>
        static Object[] GetDependObjectByPathList(Dictionary<string, List<string>> pathList)
        {
            List<Object> objCheckList = new List<Object>();
            foreach (KeyValuePair<string, List<string>> curPathPair in pathList)
            {
                foreach (string curPath in curPathPair.Value)
                {
                    objCheckList.Add(AssetDatabase.LoadMainAssetAtPath(curPath));
                }
            }

            Object[] retObjArray = new Object[objCheckList.Count];
            for (int i = 0; i < objCheckList.Count; i++)
            {
                retObjArray[i] = objCheckList[i];
            }

            return EditorUtility.CollectDependencies(retObjArray);

        }

        /// <summary>
        /// 获取UIRes/Atlas/Common目录下texture依赖资源的路径
        /// 不是UIRes/Atlas/Common该路径下的不会加入path列表中
        /// Common中加入sound资源以及NGUI所用的shader
        /// </summary>
        /// <param name="commonPathList"></param>
        /// <param name="objDepends"></param>
        static void CheckCommonDependFiles(List<string> commonPathList, Object[] objDepends)
        {
            foreach (Object dependObj in objDepends)
            {
                if (dependObj is UnityEngine.Texture)
                {
                    string commonPath = AssetDatabase.GetAssetPath(dependObj).Replace('\\', '/');
                    if (commonPath.Contains("Res/Texture/common") && !commonPathList.Contains(commonPath))
                    {
                        Debug.Log("add common ui: " + commonPath);
                        commonPathList.Add(commonPath);
                    }
                }

                if (dependObj is UnityEngine.AudioClip)
                {
                    string soundPath = AssetDatabase.GetAssetPath(dependObj);
                    if (!commonPathList.Contains(soundPath))
                    {
                        Debug.Log("add sound: " + soundPath);
                        commonPathList.Add(soundPath);
                    }
                }

                if (dependObj is UnityEngine.Shader)
                {
                    string shaderPath = AssetDatabase.GetAssetPath(dependObj);
                    if (!commonPathList.Contains(shaderPath))
                    {
                        Debug.Log("add shader: " + shaderPath);
                        commonPathList.Add(shaderPath);
                    }
                }
            }
        }

        /// <summary>
        /// 获取字体依赖资源的路径
        /// 只要是字体就会加入依赖path列表中
        /// </summary>
        /// <param name="fontPathList">字体资源的路径</param>
        /// <param name="objDepends">依赖的资源对象</param>
        static void CheckFontDependFiles(List<string> fontPathList, Object[] objDepends)
        {
            foreach (Object dependObj in objDepends)
            {
                if (dependObj is UnityEngine.Font)
                {
                    string fontPath = AssetDatabase.GetAssetPath(dependObj);
                    if (!fontPathList.Contains(fontPath))
                    {
                        Debug.Log("add font: " + fontPath);
                        fontPathList.Add(fontPath);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定目录下所依赖的资源路径
        /// </summary>
        /// <param name="checkPath">指定的目录</param>
        /// <param name="uiPathList">返回的path集合</param>
        /// <param name="objDepends">依赖的资源对象</param>
        static void CheckUIDependFiles(string checkPath, List<string> uiPathList, Object[] objDepends)
        {
            foreach (Object dependObj in objDepends)
            {
                if (dependObj is UnityEngine.Texture)
                {
                    string texPath = AssetDatabase.GetAssetPath(dependObj).Replace('\\', '/');
                    if (texPath.Contains(checkPath) && !uiPathList.Contains(texPath))
                    {
                        Debug.Log(checkPath + " :add ui depend file:" + texPath);
                        uiPathList.Add(texPath);
                    }
                }
            }
        }
    }
}


using UnityEditor;
using CloverGame.Config;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Assets.Editor
{
    public class ToolsMenu : UnityEditor.Editor
    {

        private const string SMode = "Tools/Simulation Mode";
        [MenuItem(SMode, false)]
        public static void SwithSimulationMode()
        {
            PlayerSetting.SimulateAssetBundleInEditor = !PlayerSetting.SimulateAssetBundleInEditor;
            Menu.SetChecked(SMode, PlayerSetting.SimulateAssetBundleInEditor);
            if (PlayerSetting.SimulateAssetBundleInEditor)
            {
                Debug.Log("SMODE Opened");
                //AddAllSceneToBuildingSetting();
            }
            else
            {
                Debug.Log("SMODE Closed");
               // AddAllSceneToBuildingSetting();
            }
        }

    }
}

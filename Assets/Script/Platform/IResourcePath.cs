using UnityEngine;

namespace CloverGame.Platform
{
    public abstract class  IResourcePath
    {
        public string GetPathRoot()
        {
            return Application.persistentDataPath + "/ResData";
        }

        public abstract string GetPath(string subFolder, string localName, string fileExt);
    }
}

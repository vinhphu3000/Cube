using System.Collections;
using UnityEngine;

namespace CloverGame.Resource
{
    public abstract class IInfoData
    {
    }

    public abstract class IResourceManager
    {
        public delegate void LoadBundleFinish(IInfoData infoData, GameObject obj, object param1, object param2);

        public abstract IEnumerator LoadResource(IInfoData infoData,
                                                 LoadBundleFinish delFinish,
                                                 object param1,
                                                 object param2);
    }
}

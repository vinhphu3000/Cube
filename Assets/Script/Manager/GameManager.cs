using UnityEngine;
using CloverGame.Utils;

namespace CloverGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            instance = null;
        }
    }
}

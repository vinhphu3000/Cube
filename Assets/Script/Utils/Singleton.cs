using System;

namespace CloverGame.Utils
{
    public class Singleton<T> where T : class, new()
    {
        private static T _Instance;

        static Singleton()
        {
            _Instance = new T();
        }

        public static void CreateInstance()
        {
            if (_Instance == null)
            {
                _Instance = Activator.CreateInstance<T>();
            }
        }

        public static void DestroyInstance()
        {
            if (_Instance != null)
            {
                _Instance = null;
            }
        }

        public static T getInstance()
        {
            return GetInstance();
        }

        public static T GetInstance()
        {
            if (_Instance == null)
            {
                _Instance = Activator.CreateInstance<T>();
            }

            return _Instance;
        }

        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = Activator.CreateInstance<T>();
                }
                return _Instance;
            }
        }
    }
}

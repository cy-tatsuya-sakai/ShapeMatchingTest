using UnityEngine;

namespace Samples.Sample002
{
    /// <summary>
    /// シングルトン
    /// </summary>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = FindFirstObjectByType<T>();
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if(Instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}

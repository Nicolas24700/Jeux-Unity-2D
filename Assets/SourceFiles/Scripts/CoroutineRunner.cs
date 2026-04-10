using UnityEngine;
using System.Collections;

// script for running coroutines from static contexts without needing to have a reference to a MonoBehaviour instance
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner instance;

    private static CoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                
                var go = new GameObject("_CoroutineRunner");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<CoroutineRunner>();
            }
            return instance;
        }
    }

    public static Coroutine Run(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

public sealed class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private static int _mainThreadId;
    private readonly ConcurrentQueue<Action> _actionPending = new ConcurrentQueue<Action>();
    private static bool IsMainThread => Environment.CurrentManagedThreadId == _mainThreadId;
    
    private static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null) CreateHiddenInstance();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() => CreateHiddenInstance();

    private static void CreateHiddenInstance()
    {
        if (_instance != null) return;
        var go = new GameObject(nameof(UnityMainThreadDispatcher));
        _instance = go.AddComponent<UnityMainThreadDispatcher>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;
        _mainThreadId = Environment.CurrentManagedThreadId;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        while (_actionPending.TryDequeue(out var action))
        {
            SafeAction(action);
        }
    }

    public static void EnqueueCallback(Action action)
    {
        if (action == null) return;

        if (IsMainThread)
        {
            SafeAction(action);
        }
        else
        {
            var inst = Instance;
            if (inst != null) inst._actionPending.Enqueue(action);
        }
    }
    
    private static void SafeAction(Action action)
    {
        try { action?.Invoke(); }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    public static void EnqueueCoroutine(IEnumerator routine)
    {
        if (routine == null) return;

        if (IsMainThread)
        {
            _instance?.StartCoroutine(routine);
        }
        else
        {
            var inst = Instance;
            if (inst != null) inst._actionPending.Enqueue(() => inst.StartCoroutine(routine));
        }
    }
}

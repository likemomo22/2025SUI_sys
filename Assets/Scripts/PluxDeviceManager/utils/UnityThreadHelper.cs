using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityThreading;
#if !NO_UNITY
using UnityEngine;
#endif

#if !NO_UNITY
[ExecuteInEditMode]
public class UnityThreadHelper : MonoBehaviour
#else
public class UnityThreadHelper
#endif
{
    private static UnityThreadHelper instance;
    private static readonly object syncRoot = new();

    public static void EnsureHelper()
    {
        lock (syncRoot)
        {
#if !NO_UNITY
            if (null == (object)instance)
            {
                instance = FindObjectOfType(typeof(UnityThreadHelper)) as UnityThreadHelper;
                if (null == (object)instance)
                {
                    var go = new GameObject("[UnityThreadHelper]");
                    go.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    instance = go.AddComponent<UnityThreadHelper>();
                    instance.EnsureHelperInstance();
                }
            }
#else
		    if (null == instance)
		    {
			    instance = new UnityThreadHelper();
			    instance.EnsureHelperInstance();
		    }
#endif
        }
    }

    private static UnityThreadHelper Instance
    {
        get
        {
            EnsureHelper();
            return instance;
        }
    }

    /// <summary>
    ///     Returns the GUI/Main Dispatcher.
    /// </summary>
    public static Dispatcher Dispatcher => Instance.CurrentDispatcher;

    /// <summary>
    ///     Returns the TaskDistributor.
    /// </summary>
    public static TaskDistributor TaskDistributor => Instance.CurrentTaskDistributor;

    public Dispatcher CurrentDispatcher { get; private set; }

    public TaskDistributor CurrentTaskDistributor { get; private set; }

    private void EnsureHelperInstance()
    {
        CurrentDispatcher = Dispatcher.MainNoThrow ?? new Dispatcher();
        CurrentTaskDistributor = TaskDistributor.MainNoThrow ?? new TaskDistributor("TaskDistributor");
    }

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action<ActionThread> action, bool autoStartThread)
    {
        Instance.EnsureHelperInstance();

        Action<ActionThread> actionWrapper = currentThread =>
        {
            try
            {
                action(currentThread);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        };
        var thread = new ActionThread(actionWrapper, autoStartThread);
        Instance.RegisterThread(thread);
        return thread;
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action<ActionThread> action)
    {
        return CreateThread(action, true);
    }

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action action, bool autoStartThread)
    {
        return CreateThread(thread => action(), autoStartThread);
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ActionThread CreateThread(Action action)
    {
        return CreateThread(thread => action(), true);
    }

    #region Enumeratable

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The enumeratable action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<ThreadBase, IEnumerator> action, bool autoStartThread)
    {
        Instance.EnsureHelperInstance();

        var thread = new EnumeratableActionThread(action, autoStartThread);
        Instance.RegisterThread(thread);
        return thread;
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The enumeratable action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<ThreadBase, IEnumerator> action)
    {
        return CreateThread(action, true);
    }

    /// <summary>
    ///     Creates new thread which runs the given action. The given action will be wrapped so that any exception will be
    ///     catched and logged.
    /// </summary>
    /// <param name="action">The enumeratable action which the new thread should run.</param>
    /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<IEnumerator> action, bool autoStartThread)
    {
        Func<ThreadBase, IEnumerator> wrappedAction = thread => { return action(); };
        return CreateThread(wrappedAction, autoStartThread);
    }

    /// <summary>
    ///     Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so
    ///     that any exception will be catched and logged.
    /// </summary>
    /// <param name="action">The action which the new thread should run.</param>
    /// <returns>The instance of the created thread class.</returns>
    public static ThreadBase CreateThread(Func<IEnumerator> action)
    {
        Func<ThreadBase, IEnumerator> wrappedAction = thread => { return action(); };
        return CreateThread(wrappedAction, true);
    }

    #endregion

    private readonly List<ThreadBase> registeredThreads = new();

    private void RegisterThread(ThreadBase thread)
    {
        if (registeredThreads.Contains(thread)) return;

        registeredThreads.Add(thread);
    }

#if !NO_UNITY

    private void OnDestroy()
    {
        foreach (var thread in registeredThreads)
            thread.Dispose();

        if (CurrentDispatcher != null)
            CurrentDispatcher.Dispose();
        CurrentDispatcher = null;

        if (CurrentTaskDistributor != null)
            CurrentTaskDistributor.Dispose();
        CurrentTaskDistributor = null;

        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (CurrentDispatcher != null)
            CurrentDispatcher.ProcessTasks();

        var finishedThreads = registeredThreads.Where(thread => !thread.IsAlive).ToArray();
        foreach (var finishedThread in finishedThreads)
        {
            finishedThread.Dispose();
            registeredThreads.Remove(finishedThread);
        }
    }
#endif
}
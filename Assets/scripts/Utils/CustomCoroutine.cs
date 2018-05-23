//CAW
//Adapted from http://twistedoakstudios.com/blog/Post83_coroutines-more-than-you-want-to-know

using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

    public static class MonoBehaviourExt
{
        [NotNull]
        public static Coroutine<T> StartCoroutine<T>(this MonoBehaviour obj, IEnumerator inputCoroutine)
        {
            return new Coroutine<T>(obj, inputCoroutine);
        }
    }

public class Coroutine<T>
{
    internal Coroutine(MonoBehaviour startedValue, IEnumerator inputCoroutine)
    {
        m_MonoBehaviour = startedValue;
        if (startedValue != null && startedValue.gameObject.activeInHierarchy)
        {
            coroutine = startedValue.StartCoroutine(InternalRoutine(inputCoroutine));
        }
        else
        {
            Cancel();
            Debug.LogError("Tried to start a Coroutine on " +
                           (startedValue == null
                               ? " a null MonoBehaviour "
                               : (startedValue.name + " when it is not active in the Hierarchy")));
        }
    }

    //local variable used to determine if the coroutine is still running
    private bool m_IsDone;
    private readonly MonoBehaviour m_MonoBehaviour;
    private Action<T, bool> m_OnCompleteAction;

    /// <summary>
    /// Gets the value if one has returned, otherwise it will be null, will throw an exception if we have thrown an exception
    /// </summary>
    public T Value
    {
        get
        {
            if (Exception != null)
            {
                throw Exception;
            }
            return m_ReturnVal;
        }
    }

    private T ValueSafe
    {
        get { return m_ReturnVal; }
    }

    private T m_ReturnVal;

    /// <summary>
    /// True if this coroutine was cancelled manually
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// True if we have thrown an error during execution
    /// </summary>
    public bool HasException
    {
        get { return Exception != null; }
    }

    /// <summary>
    /// the exception, if any, that was thrown
    /// </summary>
    public Exception Exception { get; private set; }

    /// <summary>
    /// Called yield return this to wait until the coroutine finishes before exiting
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public Coroutine coroutine { get; private set; }

    public bool IsActive
    {
        get { return !m_IsDone && m_MonoBehaviour != null && m_MonoBehaviour.gameObject.activeInHierarchy; }
    }

    /// <summary>
    ///call this to stop the coroutine, will throw an exception the next time the coroutine leaves a yield statement
    /// </summary>
    public void Cancel()
    {
        //set our flag
        IsCancelled = true;
        //and automatically set our error
        Exception = new CoroutineCancelledException();
        FinishCoroutine();
    }

    private IEnumerator InternalRoutine(IEnumerator coroutineToRun)
    {
        while (true)
        {
            //if we called canceled
            if (IsCancelled)
            {
                //then exit this coroutine
                yield break;
            }
            try
            {
                //otherwise run until our next yield function or the end
                if (!coroutineToRun.MoveNext())
                {
                    //if we reach the end of the coroutine bail this one
                    FinishCoroutine();
                    yield break;
                }
            }
                //catch the exception that was thrown
            catch (Exception e)
            {
                Exception = e;
                Debug.LogException(e);
                FinishCoroutine();
                yield break;
            }
            //check to see what our last yield was
            object yielded = coroutineToRun.Current;
            //if its not null and our desired value type
            if (yielded is T)
            {
                //cast it
                m_ReturnVal = (T) yielded;
            }
            //otherwise it is a unity yield statement and we should just let unity handle it
            else
            {
                //otherwise run the next portion
                yield return coroutineToRun.Current;
            }
        }
    }

    private void FinishCoroutine()
    {
        if (m_IsDone)
        {
            return;
        }
        m_IsDone = true;
        if (m_OnCompleteAction != null && Exception == null)
        {
            m_OnCompleteAction.Invoke(ValueSafe, Exception != null);
        }
    }

    /// <summary>
    /// Register for a callback once the Coroutine finishes, args passed are Value, HadError
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public Coroutine<T> OnComplete(Action<T, bool> onComplete)
    {
        m_OnCompleteAction = onComplete;
        return this;
    }
}

public class CoroutineCancelledException : Exception
{
    public CoroutineCancelledException() : base("Coroutine was cancelled")
    {

    }
}
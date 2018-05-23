using UnityEngine;
using JetBrains.Annotations;

/// <summary>
/// This allows for easy access to a singular instance of a MonoBehaviour in the unity Universe
/// Good uses : 
///     1. Single controller that only exists in the game once and typically 
///     is able to be created without needing custimazation in the editor
///         ie manager classes
/// Bad uses : 
///     1. Anything that could have uses for having multiple of them in the scenes
///         ie coins that are able to be retrieved in many places in the game
///     2. Something that would break miserably if the editor values were not 
///         set up correctly, since this will create the component upon access 
///         to the variable if it doesn't already exist in the scene
///             ie a complicated UI prefab
/// </summary>
/// <typeparam name="T">This should just pass in the name of the class of the parent script</typeparam>
public class Singleton<T> : MonoBehaviour where T:  MonoBehaviour
{
    [NotNull]
    public static T Instance
    {
        get
        {
            if (s_Instance == null && !s_IsQuitting)
            {
                s_Instance = FindObjectOfType<T>();
                if (s_Instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    s_Instance = go.AddComponent<T>();
                }
            }
            // safe to assume not null here because we don't tpyically really care what happens when the game exits
            // ReSharper disable once AssignNullToNotNullAttribute
            return s_Instance;
        }
    }

    private static T s_Instance;
    //need to track this since you can abandon objects in the scene if you instantiate after the quit/ondestroy method has been called
    // ReSharper disable once StaticMemberInGenericType
    private static bool s_IsQuitting;

    /// <summary>
    /// Returns whether an Instance exists in the scene currently without creating a new one, 
    /// VERY VERY high cost function if the instance doesnt exist so never use in an Update or OnGUI loop
    /// </summary>
    public static bool HasInstance
    {
        get
        {
            if (s_Instance != null)
            {
                s_Instance = FindObjectOfType<T>();
            }
            return s_Instance != null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        s_IsQuitting = true;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager : Singleton<OptionsManager>
{
    private readonly Dictionary<string, string> m_SelectedOptions = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
    private readonly Dictionary<string, Action<string>> m_Callbacks = new Dictionary<string, Action<string>>(StringComparer.CurrentCultureIgnoreCase);

    public void HandleOptions(Options ident, Options option, string[] badParse)
    {
        foreach (var key in option.GetKeys())
        {
            var val = option.GetValue(key);
            Debug.Log("Recieved option " + key + " = " + val);
            if (m_SelectedOptions.ContainsKey(key))
            {
                Debug.Log("Updating key " + key + " to " + val);
            }
            m_SelectedOptions[key] = val;
            Action<string> callback;
            if (m_Callbacks.TryGetValue(key, out callback))
            {
                callback(val);
            }
        }
    }

    /// <summary>
    /// Register a callback anytime a desired option was changed, if the option has already been set, onOptionSet will be called before the function returns
    /// </summary>
    /// <param name="optionToListenFor">Name of the option set in the game def file</param>
    /// <param name="onOptionSet">Callback when the option is set</param>
    public void RegisterOptionChanged(string optionToListenFor, Action<string> onOptionSet)
    {
        Action<string> callback;
        if (m_Callbacks.TryGetValue(optionToListenFor, out callback))
        {
            callback += onOptionSet;
        }
        else
        {
            callback = onOptionSet;
        }
        m_Callbacks[optionToListenFor] = callback;
        string currentVal;
        if (m_SelectedOptions.TryGetValue(optionToListenFor, out currentVal))
        {
            callback(currentVal);
        }
    }

}

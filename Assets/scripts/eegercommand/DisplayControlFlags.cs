using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
// RewardableState and DisplayControlFlags should extend a base abstract class

public class DisplayControlFlags
{
    private Dictionary<string, bool> values = new Dictionary<string, bool>();

    public DisplayControlFlags(JsonData json)
    {
        foreach (string key in ((IDictionary)json).Keys)
        {
            add(key, (bool)json[key]);
        }
    }

    public void add(string flag, bool value)
    {
        values.Add(flag, value);
    }

    public bool get(string flag)
    {
        return values[flag];
    }

    public void debug()
    {
        foreach (KeyValuePair<string, bool> entry in values)
        {
            Debug.Log(entry.Key + ": " + entry.Value);
        }
    }
}

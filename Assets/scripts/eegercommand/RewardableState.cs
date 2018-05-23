using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class RewardableState
{
    private Dictionary<string, bool> values = new Dictionary<string, bool>();
    int reward;
    //private MessagesController messages;

    public RewardableState(JsonData json, int reward)
    {
        //messages = MessagesControllerFactory.factory ();

        foreach (string key in ((IDictionary)json).Keys)
        {
            add(key, (bool)json[key]);
        }

        this.reward = reward;
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
        Debug.Log("Reward: " + this.reward);

        //foreach (KeyValuePair<string, bool> entry in values) {
        //	string message = entry.Key + ": " +  entry.Value;
        //	messages.add (message);
        //	Debug.Log (message);
        //}
    }
}
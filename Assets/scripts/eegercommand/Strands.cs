using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;

public class Strands
{
    private readonly Dictionary<int, float> m_Values = new Dictionary<int, float>();

    public Strands(JsonData json)
    {
        foreach (string key in ((IDictionary)json).Keys)
        {
            Add(int.Parse(key), json[key].ParseNumber());
        }
    }

    //Takes the current average of strings and returns their value.
    public float AverageValues()
    {
        float sum = 0;

        foreach (float value in m_Values.Values)
        {
            sum += value;
        }

        sum /= m_Values.Count;

        return sum;
    }

    public void Add(int strand, float value)
    {
        m_Values.Add(strand, value);
    }

    public float Get(int strand)
    {
        return m_Values[strand];
    }

    public int[] GetStrandLocations()
    {
        return m_Values.Keys.ToArray();
    }

    public void Debug()
    {
        foreach (KeyValuePair<int, float> entry in m_Values) {
        	string message = entry.Key + ": " +  entry.Value;
        	UnityEngine.Debug.Log (message);
        }
    }

    public bool TryGetAmplitude(int channel, out float val)
    {
        if (m_Values == null)
        {
            val = 0f;
            return false;
        }
        return m_Values.TryGetValue(channel, out val);
    }
}

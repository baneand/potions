using System.Collections;
using System.Collections.Generic;
using LitJson;

public class Options
{
    private readonly Dictionary<string, string> m_Values = new Dictionary<string, string>();

    public Options(JsonData json)
    {
        foreach (string key in (json as IDictionary).Keys)
        {
            m_Values.Add(key, (string)json[key]);
        }
    }

    public IEnumerable<string> GetKeys()
    {
        return m_Values.Keys;
    }

    public bool TryGetValue(string key, out string value)
    {
        return m_Values.TryGetValue(key, out value);
    }

    public string GetValue(string key)
    {
        return m_Values[key];
    }
}

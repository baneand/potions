using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class Frequencies
{
    private class FreqRange
    {
        public FreqRange(int freq1, int freq2)
        {
            Min = Mathf.Min(freq1, freq2);
            Max = Mathf.Max(freq1, freq2);
        }
        public readonly int Min;
        public readonly int Max;
    }

    private readonly Dictionary<int, FreqRange> m_FreqRanges = new Dictionary<int, FreqRange>();

    public Frequencies(JsonData data)
    {
        if (data == null || !data.IsArray)
        {
            return;
        }
        for (int i = 0; i < data.Count/2; i++)
        {
            try
            {
                var freq1 = JsonUtil.ParseInteger(data[i*2]);
                var freq2 = JsonUtil.ParseInteger(data[i*2 + 1]);
                m_FreqRanges.Add(i, new FreqRange(freq1, freq2));
            }
            catch (Exception)
            {
                Debug.LogWarning("Could not parse data for i " + i);
            }
        }
    }

    public int Count
    {
        get { return m_FreqRanges.Count; }
    }

    public bool TryGetRange(int strand, out int min, out int max)
    {
        FreqRange freqRange;
        if (m_FreqRanges.TryGetValue(strand, out freqRange))
        {
            min = freqRange.Min;
            max = freqRange.Max;
            return true;
        }
        min = max = -1;
        return false;
    }

    public override string ToString()
    {
        string output = string.Empty;
        foreach (var range in m_FreqRanges)
        {
            output += string.Format("{0} = {1} - {2}\n", range.Key, range.Value.Min, range.Value.Max);
        }
        return output;
    }
}

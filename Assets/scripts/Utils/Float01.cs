using System;
using UnityEngine;

public class Float01
{
    private float m_Value;
    private float m_MinValue;
    private float m_MaxValue;

    public bool AutoIncreaseMaxValue { get; set; }

    public Float01(float value) : this(value, 0, 2)
    {
    }

    public Float01(float value, float min, float max)
    {
        UpdateMinMax(min, max);
        m_Value = value;
    }

    public void UpdateValueDirectly(float value)
    {
        m_Value = value;
        if (AutoIncreaseMaxValue)
        {
            m_MaxValue = Mathf.Max(m_Value, m_MaxValue);
        }
    }

    public void UpdateValuePercentage(float percent)
    {
        if (percent < 0f || percent > 1f)
        {
            throw new ArgumentException("Percent should be between 0 and 1");
        }
        m_Value = percent * (m_MaxValue - m_MinValue);
    }

    public void UpdateMinMax(float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min can not be greater than max");
        }
        m_MinValue = min;
        m_MaxValue = max;
    }

    public float GetValue()
    {
        m_Value = Mathf.Clamp(m_Value, m_MinValue, m_MaxValue);
        if (Mathf.Approximately(m_MinValue, m_MaxValue))
        {
            return 1f;
        }
        return m_Value / (m_MaxValue - m_MinValue);
    }
}

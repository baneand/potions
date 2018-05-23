using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public interface IGameDefItem
{
}

public interface IOptionSection : IGameDefItem
{
    string GamedefName { get; }
    string OptionLayout { get; }
    string HelpString { get; }
}

public interface IDefSection : IGameDefItem
{
    string GamedefName { get; }
    string Type { get; }
    string[] Options { get; }
}

public interface IIdentSection : IGameDefItem
{
    string Title { get; }
    string Name { get; }
    string Code { get; }
}

[Serializable]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
public class LimitInfo
{
    public static LimitInfo[] GetDefaultLimitSetup()
    {
        return new[]
        {
            new LimitInfo("inhibits", 0, 2, 2),
            new LimitInfo("rewards",  0, 1, 1),
            new LimitInfo("sounds",   0, 1, 1),
            new LimitInfo("streams",  3, 3, 3)
        };
    }

    public LimitInfo(string name, int min, int max, int defaultValue)
    {
        m_Name = name;
        m_Min = min;
        m_Max = max;
        m_DefaultValue = defaultValue;
    }

    [SerializeField] private string m_Name;
    [SerializeField] private int m_Max;
    [SerializeField] private int m_Min;
    [SerializeField] private int m_DefaultValue;

    public string Name
    {
        get { return m_Name; }
    }

    public int Max
    {
        get { return m_Max; }
    }

    public int Min
    {
        get { return m_Min; }
    }

    public int DefaultValue
    {
        get { return m_DefaultValue; }
    }
}

public interface ILimitSection : IGameDefItem
{
    LimitInfo[] GetLimitInfo();
}

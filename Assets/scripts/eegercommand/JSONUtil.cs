using System;
using UnityEngine;
using System.Collections.Generic;
using LitJson;

public static class JsonUtil
{
    public static int ParseInteger(JsonData json)
    {
        if (json == null)
        {
            return 0;
        }
        if (json.IsInt)
        {
            return (int)json;
        }
        if (json.IsString)
        {
            int value;
            if (int.TryParse((string) json, out value))
            {
                return value;
            }
        }
        if (json.IsLong)
        {
            return (int) (long) json;
        }
        if (json.IsDouble)
        {
            return (int) (double) json;
        }
        Debug.LogWarning("Could not parse json data to int " + json);
        return 0;
    }

    public static List<int> ParseIntegerList(this JsonData json)
    {
        List<int> numbers = new List<int>();

        if (json != null && json.IsArray)
        {
            for (int i = 0; i < json.Count; i++)
            {
                numbers.Add((int)json[i]);
            }
        }
        return numbers;
    }

    public static string[] ParseStringArray(this JsonData json)
    {
        if (json == null || !json.IsArray)
        {
            return null;
        }
        try
        {
            var returnVal = new string[json.Count];
            for (int i = 0; i < json.Count; i++)
            {
                returnVal[i] = (string) json[i];
            }
            return returnVal;
        }
        catch
        {
            Debug.LogWarning("Could not parse data into string array " + json.ToJson());
            return null;
        }
    }

    public static float ParseNumber(this JsonData json)
    {
        if (json == null)
        {
            return 0f;
        }
        if (json.IsString)
        {
            return float.Parse((string) json);
        }
        if (json.IsDouble)
        {
            return (float)(double)json;
        }
        if (json.IsInt)
        {
            return (int)json;
        }
        return 0f;
    }

    public static void Print(this JsonData json)
    {
        Debug.Log(json.ToString());
    }

    public static bool TryConvertJson(string json, out JsonData data)
    {
        try
        {
            data = JsonMapper.ToObject(json);
            return true;
        }
        catch
        {
            data = null;
            return false;
        }
    }

    public static bool TryParseString(this JsonData data, string key, out string value)
    {
        try
        {
            value = (string) data[key];
            return true;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }
}
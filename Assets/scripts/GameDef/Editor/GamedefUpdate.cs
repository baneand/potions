using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GamedefUpdate : EditorWindow
{
    [MenuItem("Eeger/Update game def file for open scene")]
    public static void OpenWindow()
    {
        GetWindow<GamedefUpdate>();
    }

    private string m_GameDefLocation;

    // ReSharper disable once UnusedMember.Local
    private void OnGUI()
    {
        if (GUILayout.Button("Select Game def file"))
        {
            m_GameDefLocation = EditorUtility.OpenFilePanel("Select game def file", Application.dataPath + "/../../",
                "gamedef");
        }
        GUI.enabled = !string.IsNullOrEmpty(m_GameDefLocation);
        if (GUILayout.Button("Update selected game def file"))
        {
            UpdateGamedefFile(m_GameDefLocation);
        }
    }

    public class GameDefFile
    {
        const string Options = "[OPTIONS]";
        const string Defs = "[DEFS]";
        const string Help = "[HELP]";
        const string Ident = "[IDENT]";
        const string Limits = "[LIMITS]";

        public GameDefFile(string filePath)
        {
            var fileContents = File.ReadAllLines(filePath);
            var superList = new List<string[]>();
            int startValue = 0;
            for (int i = 0; i < fileContents.Length; i++)
            {
                string line = fileContents[i].Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (i > startValue)
                    {
                        var chunk = new string[i - startValue];
                        Array.Copy(fileContents, startValue, chunk, 0, i - startValue);
                        superList.Add(chunk);
                        startValue = i;
                    }
                }
            }
            if (fileContents.Length > startValue)
            {
                var chunk = new string[fileContents.Length - startValue];
                Array.Copy(fileContents, startValue, chunk, 0, fileContents.Length - startValue);
                superList.Add(chunk);
            }
            m_LeftOver = new List<string>();
            m_Defs = new List<string>();
            m_Help = new List<string>();
            m_Options = new List<string>();
            m_Ident = new List<string>();
            m_Limits = new List<string>();
            foreach (string[] chunk in superList)
            {
                var chunkKey = chunk[0].Trim();
                switch (chunkKey)
                {
                    case Defs:
                        m_Defs.AddRange(chunk);
                        break;
                    case Options:
                        m_Options.AddRange(chunk);
                        break;
                    case Help:
                        m_Help.AddRange(chunk);
                        break;
                    case Ident:
                        m_Ident.AddRange(chunk);
                        break;
                    case Limits:
                        m_Limits.AddRange(chunk);
                        break;
                    default:
                        m_LeftOver.AddRange(chunk);
                        break;
                }
            }
            TrimBottomOfList(m_LeftOver);
            TrimBottomOfList(m_Defs);
            TrimBottomOfList(m_Help);
            TrimBottomOfList(m_Options);
            TrimBottomOfList(m_Ident);
            TrimBottomOfList(m_Limits);
        }

        private readonly List<string> m_Options;
        private readonly List<string> m_Defs;
        private readonly List<string> m_Help;
        private readonly List<string> m_Ident;
        private readonly List<string> m_Limits;
        private readonly List<string> m_LeftOver;

        public void UpdateItem(IGameDefItem item)
        {
            UpdateOptionSection(item as IOptionSection);
            UpdateDefSection(item as IDefSection);
            UpdateIdentSection(item as IIdentSection);
            UpdateLimitsSection(item as ILimitSection);
        }

        private void UpdateOptionSection(IOptionSection item)
        {
            if (item == null)
            {
                return;
            }
            UpdateAddList(item.GamedefName, item.OptionLayout, m_Options);
            UpdateAddList("options." + item.GamedefName, item.HelpString, m_Help);
        }

        private void UpdateDefSection(IDefSection item)
        {
            if (item == null)
            {
                return;
            }
            var options = item.Options;
            var desiredValue = item.Type + "['";
            desiredValue += options.Aggregate((current, option) => current + "', '" + option);
            desiredValue += "']";
            UpdateAddList(item.GamedefName, desiredValue, m_Defs);
        }

        private void UpdateIdentSection(IIdentSection item)
        {
            if (item == null)
            {
                return;
            }
            UpdateAddList("title", item.Title, m_Ident);
            UpdateAddList("name", item.Name, m_Ident);
            UpdateAddList("code", item.Code, m_Ident);
        }

        private void UpdateLimitsSection(ILimitSection item)
        {
            if (item == null)
            {
                return;
            }
            var limits = item.GetLimitInfo();
            if (limits == null)
            {
                return;
            }
            for (int i = 0; i < limits.Length; i++)
            {
                var limitInfo = limits[i];
                string value = string.Format("I,{0},{1},{2},{3}", limitInfo.Min, limitInfo.Max, 3000 + i,
                    limitInfo.DefaultValue);
                UpdateAddList(limitInfo.Name, value, m_Limits);
            }
        }

        private static void UpdateAddList(string keyName, string value, List<string> list)
        {
            if (string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(value))
            {
                Debug.LogWarning("Will not upgrade item with key " + keyName + " to value " + value);
                return;
            }
            string desiredLine = keyName + "=" + value;
            for (int i = 0; i < list.Count; i++)
            {
                var option = list[i];
                if (option.StartsWith(keyName))
                {
                    list[i] = desiredLine;
                    return;
                }
            }
            list.Add(desiredLine);
        }

        private static void TrimBottomOfList(List<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }
            while (string.IsNullOrEmpty(list[list.Count - 1].Trim()))
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        public string[] GetFinalContents()
        {
            var output = new List<string>();
            output.AddRange(m_Ident);
            output.Add("");
            output.AddRange(m_Limits);
            output.Add("");
            output.AddRange(m_Options);
            output.Add("");
            output.AddRange(m_Defs);
            output.Add("");
            output.AddRange(m_Help);
            if (m_LeftOver.Count > 0)
            {
                output.Add("");
                output.AddRange(m_LeftOver);
            }
            return output.ToArray();
        }
    }

    private static void UpdateGamedefFile(string filePath)
    {
        var gameDefFile = new GameDefFile(filePath);
        var allItems = FindAllGameDefItems();
        foreach (var item in allItems)
        {
            gameDefFile.UpdateItem(item);
        }
        File.WriteAllLines(filePath, gameDefFile.GetFinalContents());
    }

    private static IGameDefItem[] FindAllGameDefItems()
    {
        var allPossibleItems = FindObjectsOfType<MonoBehaviour>();
        List<IGameDefItem> gameDefItems = new List<IGameDefItem>();
        foreach (var item in allPossibleItems)
        {
            var gameDefItem = item as IGameDefItem;
            if (gameDefItem != null)
            {
                gameDefItems.Add(gameDefItem);
            }
        }
        return gameDefItems.ToArray();
    }
}

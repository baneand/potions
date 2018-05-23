using UnityEngine;
using UnityEngine.UI;
using ClientSockets;
using System.Collections.Generic;
using LitJson;

public class Recorder : MonoBehaviour
{
    public Text textObject;

    List<SavedInfo> infos = new List<SavedInfo>();
    SimpleClient client;

    private void Start ()
    {
        client = new SimpleClient("127.0.0.1", 5670, HandleMessage);
        client.Connect();
   	}

    private class SavedInfo
    {
        public SavedInfo(float time, string command)
        {
            Time = time;
            JsonUtil.TryConvertJson(command, out Command);
        }

        public readonly float Time;
        public readonly JsonData Command;

        public JsonData ToJsonData()
        {
            var data = new JsonData();
            data["time"] = Time;
            data["message"] = Command;
            return data;
        }
    }
    List<string> commands = new List<string>();
    private void HandleMessage(string message)
    {
        //Debug.Log("Got message " + message);
        commands.Add(message);
    }

    void Update()
    {
        while(commands.Count > 0)
        {
            var command = commands[0];
            if (infos == null)
            {
                infos = new List<SavedInfo>();
            }
            infos.Add(new SavedInfo(Time.realtimeSinceStartup, command));
            commands.RemoveAt(0);
        }
        if (textObject != null)
        {
            textObject.text = "Count is " + infos.Count;
        }
        if((int)Time.realtimeSinceStartup % 30 == 0)
        {
            PrintJson();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PrintJson();
        }
    }

    void PrintJson()
    {
        JsonData data = new JsonData();
        JsonData savedInfo = new JsonData();
        savedInfo.SetJsonType(JsonType.Array);
        for (int i = 0; i < infos.Count; i++)
        {
            var info = infos[i];
            savedInfo.Add(info.ToJsonData());
        }
        data["saved_info"] = savedInfo;
        Debug.Log(data.ToJson());
    }

    void OnApplicationQuit()
    {
        PrintJson();
    }
}

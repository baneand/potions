//#define DEBUG

using System;
using UnityEngine;
using System.Collections.Generic;
using ClientSockets;
using JetBrains.Annotations;

// Main class that connects the unity game socket to the EEGer Socket to read data.
// Also Contains a Text Spoof inorder to debug and test on Unity without EEger data.

public class EegerCommand : Singleton<EegerCommand>
{
#if UNITY_EDITOR
    [SerializeField] private TextAsset m_SpoofedTextAsset;

    private class InEditorSpoof
    {
        private class EditorStep
        {
            public EditorStep(LitJson.JsonData data)
            {
                Message = data["message"].ToJson();
                Time = data["time"].ParseNumber();
            }

            public readonly string Message;
            public readonly float Time;
        }

        public InEditorSpoof(string text)
        {
            LitJson.JsonData data;
            if (!JsonUtil.TryConvertJson(text, out data))
            {
                return;
            }
            var savedInfo = data["saved_info"];

            m_Steps = new EditorStep[savedInfo.Count];
            for (var i = 0; i < savedInfo.Count; i ++)
            {
                try
                {
                    m_Steps[i] = new EditorStep(savedInfo[i]);
                }
                catch (Exception)
                {
                    Debug.LogError("Could not load step " + i);
                }
            }
        }

        private readonly EditorStep[] m_Steps;
        private int m_CurrentPosition;
        private float m_TimeOffset;

        public string Update()
        {
            if (m_CurrentPosition < 0 || m_Steps == null || m_CurrentPosition >= m_Steps.Length)
            {
                return null;
            }
            var currStep = m_Steps[m_CurrentPosition];
            if (currStep == null)
            {
                m_CurrentPosition ++;
                return null;
            }
            if (Time.realtimeSinceStartup > currStep.Time - m_TimeOffset)
            {
                m_CurrentPosition++;
                return currStep.Message;
            }
            return null;
        }

        public void Reset()
        {
            m_TimeOffset = Time.realtimeSinceStartup;
            m_CurrentPosition = 0;
        }
    }

    private InEditorSpoof m_InEditorSpoof;

    [ContextMenu("Reset in game running")]
    // ReSharper disable once UnusedMember.Local
    private void EditorSpoofReset()
    {
        if (m_InEditorSpoof != null)
        {
            m_InEditorSpoof.Reset();
        }
    }

#endif

    private readonly CommandReceiver m_Receiver = new CommandReceiver();

    [SerializeField] private string m_ServerIpAddress = "127.0.0.1";
    [SerializeField] private int m_PortNumber = 5670;
    [SerializeField]
    [Tooltip("Once we lose a connection with the server, this is how long we will wait before retrying to connect")]
    private float m_TimeBeforeRetry = 2f;
    private SimpleClient m_Client;
    private float m_InitialFailTime;
    //Subscribers to messages received by the CommandHandler controller. 
    [NotNull]
    private readonly List<IMessageSubscriber> m_MessageSubscribers = new List<IMessageSubscriber>();
    [NotNull]
    private readonly List<IRewardSubscriber> m_RewardSubscribers = new List<IRewardSubscriber>();


    //These functions add and remove subscribers from the CommandHandler controller
    public void AddSubscriber(IMessageSubscriber subscriber)
    {
        if (subscriber == null)
        {
            return;
        }
        m_MessageSubscribers.Add(subscriber);
    }

    public void AddSubscriber(IRewardSubscriber subscriber)
    {
        if (subscriber == null)
        {
            return;
        }
        m_RewardSubscribers.Add(subscriber);
        subscriber.SetIsSubscribed(true);
    }

    public void RemoveSubscriber(IMessageSubscriber subscriber)
    {
        m_MessageSubscribers.Remove(subscriber);
    }

    public void RemoveSubscriber(IRewardSubscriber subscriber)
    {
        m_RewardSubscribers.Remove(subscriber);
        subscriber.SetIsSubscribed(false);
        subscriber.ResetSubscriber();
    }

    public void Register(string command, CommandExecutor.CommandHandler handler)
    {
        m_Receiver.RegisterCommandHandler(command, handler);
    }

    private void RegisterCommandHandlers ()
    {
        CommandExecutor.CommandHandler amplitude = args =>
        {
            var strands = (Strands)args[0];
            UpdateAmplitudeValue(strands.AverageValues());
        };
        m_Receiver.RegisterCommandHandler(CommandReceiver.AMPLITUDE, amplitude);

        CommandExecutor.CommandHandler exit = args =>
        {
            m_Client.Disconnect();
            Application.Quit();
        };
        m_Receiver.RegisterCommandHandler(CommandReceiver.EXIT, exit);

        CommandExecutor.CommandHandler reward = args =>
        {
            UpdateRewards();
        };
        m_Receiver.RegisterCommandHandler(CommandReceiver.REWARD, reward);

        CommandExecutor.CommandHandler threshold = args =>
        {
            var strands = (Strands)args[0];
            UpdateThresholdValue(strands.AverageValues());
        };
        m_Receiver.RegisterCommandHandler(CommandReceiver.THRESHOLD, threshold);
    }

    //for web player, gets called via the SendMessage interface with Unity, requires it being on a GameObject named Canvas
    private void MessageListener(string message)
    {
		m_Receiver.Receive (message);
	}   

    private void Start()
    {
        RegisterCommandHandlers();
#if UNITY_EDITOR
        if (m_SpoofedTextAsset != null)
        {
            m_InEditorSpoof = new InEditorSpoof(m_SpoofedTextAsset.text);
            return;
        }
#endif
        m_Client = new SimpleClient(m_ServerIpAddress, m_PortNumber, MessageListener);
        m_Client.Connect();
    }

    private void Update ()
    {
#if UNITY_EDITOR
        if(m_InEditorSpoof != null)
        {
            var textToSpoof = m_InEditorSpoof.Update();
            if(textToSpoof != null)
            {
                MessageListener(textToSpoof);
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Input.GetKey(KeyCode.RightShift))
            {
                m_Receiver.ParsePause(null);   
            }
            else
            {
                m_Receiver.ParseRun(null);
            }
        }
#endif
        m_Receiver.Process ();

        if(m_Client != null && !m_Client.isConnectedToServer)
        {
            if(m_InitialFailTime <= 0)
            {
                m_InitialFailTime = Time.realtimeSinceStartup;
            }
            else if (Time.realtimeSinceStartup > m_InitialFailTime + m_TimeBeforeRetry)
            {
                m_Client.ReConnect();
                m_InitialFailTime = -1f;
            }
        }
        else
        {
            m_InitialFailTime = -1f;
        }
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        if(m_Client != null && m_Client.isConnectedToServer)
        {
            m_Client.Disconnect();
        }
    }

    //Update the changed amplitude value to a subscriber
    private void UpdateAmplitudeValue(float value)
    {
        foreach (var subscriber in m_MessageSubscribers)
        {
            subscriber.UpdateData(value, EegerMessageType.AMPLITUDE);
        }
    }

    //Update the changed threshold value to a subscriber
    private void UpdateThresholdValue(float value)
    {
        foreach (var subscriber in m_MessageSubscribers)
        {
            subscriber.UpdateData(value, EegerMessageType.THRESHOLD);
        }
    }

    //Update all subscribers to the reward event
    private void UpdateRewards()
    {
        foreach (var subscriber in m_RewardSubscribers)
        {
            if (subscriber != null)
            {
                subscriber.ActivateReward();
            }
        }
    }


}

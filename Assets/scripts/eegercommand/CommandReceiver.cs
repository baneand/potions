//#define DEBUG_COMMANDS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CommandReceiver
{
    private readonly Queue m_Commands = new Queue();
    private readonly CommandExecutor m_CommandExecutor;
    public const string AMPLITUDE = "A";
    public const string BLUE = "BLUE";
    public const string END = "END";
    public const string EXIT = "EXIT";
    public const string FRQ = "FRQ";
    public const string GREEN = "GREEN";
    public const string LAYOUT = "LAY";
    //Get all options through options manager
    private const string Options = "OPT";
    public const string OVERALL = "OR";
    public const string PAUSE = "PAUSE";
    public const string PERIPHERAL = "PERI";
    public const string REWARD = "REW";
    public const string RUN = "RUN";
    public const string SCALE = "SCALE";
    public const string THRESHOLD = "T";
    public const string TIM = "TIM";
    //Get all volume events through audio manager
    private const string Volume = "V";
    public const string EVENT = "EVENT";

    public CommandReceiver()
    {
        m_CommandExecutor = new CommandExecutor();
    }

    public void RegisterCommandHandler(string command, CommandExecutor.CommandHandler handler)
    {
        m_CommandExecutor.registerCommandHandler(command, handler);
    }

    public void Receive(string command)
    {
        m_Commands.Enqueue(command);
    }

    public void Process()
    {
        while (m_Commands.Count > 0)
        {
            string command = (string) m_Commands.Dequeue();
            Parse(command);
        }
    }
    // Reading the Json file
    public void Parse(string command)
    {
        JsonData commandJson;
        string commandName;
        if (!JsonUtil.TryConvertJson(command, out commandJson) || commandJson == null ||
            !commandJson.TryParseString("cmd", out commandName))
        {
            Debug.LogError("Can not parse CommandHandler named " + command);
            return;
        }

        switch (commandName)
        {
            case AMPLITUDE:
                DebugLogMessage("Amplitude CommandHandler received: ", command);
                ParseAmplitude(commandJson);
                break;
            case BLUE:
                DebugLogMessage("Blue CommandHandler received: ", command);
                ParseBlue(commandJson);
                break;
            case END:
                DebugLogMessage("End CommandHandler received: ", command);
                ParseEnd(commandJson);
                break;
            case EXIT:
                DebugLogMessage("Exit CommandHandler received: ", command);
                ParseExit(commandJson);
                break;
            case FRQ:
                DebugLogMessage("Exit CommandHandler received: ", command);
                ParseFrq(commandJson);
                break;
            case GREEN:
                DebugLogMessage("Green CommandHandler received: ", command);
                ParseGreen(commandJson);
                break;
            case LAYOUT:
                DebugLogMessage("Layout CommandHandler received: ", command);
                ParseLayout(commandJson);
                break;
            case Options:
                DebugLogMessage("Options CommandHandler received: ", command);
                ParseOptions(commandJson);
                break;
            case OVERALL:
                DebugLogMessage("Overall CommandHandler received: ", command);
                ParseOverall(commandJson);
                break;
            case PAUSE:
                DebugLogMessage("Pause CommandHandler received: ", command);
                ParsePause(commandJson);
                break;
            case PERIPHERAL:
                DebugLogMessage("Peripheral CommandHandler received: ", command);
                ParsePeripheral(commandJson);
                break;
            case REWARD:
                DebugLogMessage("Reward CommandHandler received: ", command);
                ParseReward(commandJson);
                break;
            case RUN:
                DebugLogMessage("Run CommandHandler received: ", command);
                ParseRun(commandJson);
                break;
            case SCALE:
                DebugLogMessage("Scale CommandHandler received: ", command);
                ParseScale(commandJson);
                break;
            case THRESHOLD:
                DebugLogMessage("Threshold CommandHandler received: ", command);
                ParseThreshold(commandJson);
                break;
            case TIM:
                ParseTim(commandJson);
                break;
            case Volume:
                DebugLogMessage("Volume CommandHandler received: ", command);
                ParseVolume(commandJson);
                break;
            case EVENT:
                DebugLogMessage("Event CommandHandler recieved: ", command);
                ParseEvent(commandJson);
                break;
            default:
                DebugLogMessage("Unexpected CommandHandler received: ", command);
                break;
        }
    }

    [System.Diagnostics.Conditional("DEBUG_COMMANDS")]
    private void DebugLogMessage(string message, string actualText)
    {
        Debug.Log(message + " " + actualText);
    }

    private void ParseFrq(JsonData jsonData)
    {
        var frequencies = new Frequencies(jsonData["frequencies"]);
        m_CommandExecutor.Frequency(frequencies);
    }

    public void ParseAmplitude(JsonData json)
    {
        Strands strands = new Strands(json["strands"]);
        RewardableState state = new RewardableState(json["state"], 0);
        //TODO: hgame.py "A" does not seem to contain a percent reward
        m_CommandExecutor.Amplitude(strands, state);
    }

    public void ParseBlue(JsonData json)
    {
        m_CommandExecutor.Blue();
    }

    public void ParseEnd(JsonData json)
    {
        m_CommandExecutor.End();
    }

    public void ParseExit(JsonData json)
    {
        m_CommandExecutor.Exit();
    }

    public void ParseGreen(JsonData json)
    {
        m_CommandExecutor.Green();
    }

    public void ParseLayout(JsonData json)
    {
        List<int> types = json["types"].ParseIntegerList();
        int offset = JsonUtil.ParseInteger(json["offset"]);
        Debug.Log("Offset is " + offset);
        m_CommandExecutor.Layout(types, offset);
    }

    public void ParseOptions(JsonData json)
    {
        Options ident = new Options(json["ident"]);
        Options options = new Options(json["options"]);
        string[] unparsedOptions = json["bad_parsing"].ParseStringArray();
        OptionsManager.Instance.HandleOptions(ident, options, unparsedOptions);
    }

    public void ParseOverall(JsonData json)
    {
        int overallPercent = JsonUtil.ParseInteger(json["percent"]);
        RewardableState overallState = new RewardableState(json["state"], overallPercent);
        int numberRewards = JsonUtil.ParseInteger(json["numberRewards"]);
        RewardStrands strands = new RewardStrands(json["strands"]);
        m_CommandExecutor.Overall(overallState, numberRewards, strands);
    }

    public void ParsePause(JsonData json)
    {
        m_CommandExecutor.Pause();
    }

    public void ParsePeripheral(JsonData json)
    {
        int relativeTime = JsonUtil.ParseInteger(json["relativeTime"]);
        int subchannel = JsonUtil.ParseInteger(json["subchannel"]);
        float value = json["value"].ParseNumber();
        float minValue = json["minValue"].ParseNumber();
        float maxValue = json["maxValue"].ParseNumber();
        float eegFullScale = json["eegFullScale"].ParseNumber();
        float eegSmooth = json["eegSmooth"].ParseNumber();
        DisplayControlFlags flags = new DisplayControlFlags(json["control"]);
        m_CommandExecutor.Peripheral(relativeTime, subchannel, value, minValue, maxValue, eegFullScale, eegSmooth, flags);
    }
    
    public void ParseReward(JsonData json)
    {
        int code = JsonUtil.ParseInteger(json["code"]);
        m_CommandExecutor.Reward(code);
    }

    public void ParseRun(JsonData json)
    {
        m_CommandExecutor.Run();
    }

    public void ParseScale(JsonData json)
    {
        int numberStrands = JsonUtil.ParseInteger(json["numberStrands"]);
        Strands strands = new Strands(json["strands"]);
        m_CommandExecutor.Scale(numberStrands, strands);
    }

    public void ParseThreshold(JsonData json)
    {
        Strands strands = new Strands(json["strands"]);
        m_CommandExecutor.Threshold(strands);
    }

    public void ParseTim(JsonData json)
    {
        m_CommandExecutor.Tim();
    }

    public void ParseVolume(JsonData json)
    {
        Strands strands = new Strands(json["strands"]);        
        AudioManager.Instance.HandleAudioEvent(strands);
    }

    public void ParseEvent(JsonData json)
    {
        int code = JsonUtil.ParseInteger(json["code"]);
        m_CommandExecutor.Event(code);
    }
}
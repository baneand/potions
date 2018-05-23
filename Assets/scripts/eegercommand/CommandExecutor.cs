using System.Collections.Generic;

public class CommandExecutor
{
    public delegate void CommandHandler(params object[] args);

    private readonly Dictionary<string, CommandHandler> m_Commands = new Dictionary<string, CommandHandler>();

    public void registerCommandHandler(string command, CommandHandler handler)
    {
        CommandHandler currentHandler;
        if (!m_Commands.TryGetValue(command, out currentHandler))
        {
            currentHandler = handler;
        }
        else
        {
            currentHandler += handler;
        }
        m_Commands[command] = currentHandler;
    }

    public void Amplitude(Strands strands, RewardableState state)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.AMPLITUDE, out currentHandler))
        {
            currentHandler(strands, state);
        }
    }

    public void Blue()
    {

    }

    public void End()
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.END, out currentHandler))
        {
            currentHandler();
        }
    }

    public void Exit()
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.EXIT, out currentHandler))
        {
            currentHandler();
        }
    }

    public void Green()
    {

    }

    public void Layout(List<int> types, int offset)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.LAYOUT, out currentHandler))
        {
            currentHandler(types, offset);
        }
    }

    public void Overall(RewardableState state, int numberRewards, RewardStrands strands)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.OVERALL, out currentHandler))
        {
            currentHandler(state, numberRewards, strands);
        }
    }

    public void Peripheral(int relativeTime, int subchannel, float value, float minValue, float maxValue,
        float eegFullScale, float eegSmooth, DisplayControlFlags flags)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.PERIPHERAL, out currentHandler))
        {
            currentHandler(relativeTime, subchannel, value, minValue, maxValue, eegFullScale,
            eegSmooth, flags);
        }
    }

    public void Pause()
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.PAUSE, out currentHandler))
        {
            currentHandler();
        }
    }

    public void Reward(int code)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.REWARD, out currentHandler))
        {
            currentHandler(code);
        }
        Metrics.Instance().logReward(code);
    }

    public void Run()
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.RUN, out currentHandler))
        {
            currentHandler();
        }
    }

    public void Scale(int numberStrands, Strands strands)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.SCALE, out currentHandler))
        {
            currentHandler(numberStrands, strands);
        }
    }

    public void Threshold(Strands strands)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.THRESHOLD, out currentHandler))
        {
            currentHandler(strands);
        }
    }

    public void Tim()
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.TIM, out currentHandler))
        {
            currentHandler();
        }
    }

    public void Frequency(Frequencies frequencies)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.FRQ, out currentHandler))
        {
            currentHandler(frequencies);
        }
    }

    public void Event(int code)
    {
        CommandHandler currentHandler;
        if (m_Commands.TryGetValue(CommandReceiver.EVENT, out currentHandler))
        {
            currentHandler(code);
        }
    }
}
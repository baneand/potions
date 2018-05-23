
using System;

public class FrequencyManager : Singleton<FrequencyManager>
{
    public Frequencies LatestFrequencies { get; private set; }

    private Action<Frequencies> m_Callbacks;

    private void Awake()
    {
        EegerCommand.Instance.Register(CommandReceiver.FRQ, HandleFrequency);
    }

    private void HandleFrequency(params object[] args)
    {
        if (args == null || args.Length < 1)
        {
            return;
        }
        LatestFrequencies = args[0] as Frequencies;
        if (m_Callbacks != null)
        {
            m_Callbacks(LatestFrequencies);
        }
    }

    public void RegisterFrequnciesChange(Action<Frequencies> callback)
    {
        m_Callbacks += callback;
    }
}

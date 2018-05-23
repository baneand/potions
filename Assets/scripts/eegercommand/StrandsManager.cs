using System;
using UnityEngine;
using System.Collections.Generic;

public enum StrandType
{
    Raw = 1,
    Inhibit = 2,
    Reward = 3,
    Monitor = 4,
    Display = 5
}

public class StrandsManager : Singleton<StrandsManager>
{
    private interface IGameStrand
    {
        void UpdateFrequencies(int min, int max);
        float Amplitude { get; set; }
        float Threshold { get; set; }
        void SetHighInhibitValue(bool isHighInhibit);
    }

    
    public bool IsInRewardableState
    {
        get
        {
            foreach (var gameStrand in m_GameStrands)
            {
                if (gameStrand.Strand == StrandType.Reward && gameStrand.AverageValue.GetValue() < .5f)
                {
                    return false;
                }
                if (gameStrand.Strand == StrandType.Inhibit && gameStrand.AverageValue.GetValue() > .5f)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static StrandType ConvertStrandIntToChannelType(int strandType)
    {
        return (StrandType) (strandType >> 5);
    }

    public static Color ConvertStandToColor(int strandType)
    {
        return new Color((strandType & 48) * 4 /255f, (strandType & 12) * 4 / 255f, (strandType & 3) * 4 / 255f, 1f); 
    }

    public static Color ConvertStrandIntToColor(int strandInt)
    {
        Debug.Log(strandInt + " " + ConvertStrandIntToChannelType(strandInt));
        return Color.white;
    }

    public class GameStrand : IGameStrand
    {
        public GameStrand(StrandType strandType, int strandPosition)
        {
            m_StrandPosition = strandPosition;
            m_StrandType = strandType;
        }

        private readonly StrandType m_StrandType;
        private readonly int m_StrandPosition;
        private Action<Float01> m_AmplitudeCallbacks;
        private float m_Amplitude;
        //This is only used if the strand type is Inhibit
        private bool m_IsHighInhibit;
        private float m_Threshold = 1f;
        private readonly Float01 m_AverageValue01 = new Float01(0f, 0f, 2f);

        public int MinFreq { get; private set; }
        public int MaxFreq { get; private set; }

        public Color GetStrandColor()
        {
            return ColorStrandManager.Instance.GetColorForFrequency((MaxFreq + MinFreq) / 2);
        }

        public StrandType Strand
        {
            get { return m_StrandType; }
        }

        public void RegisterAmplitudeCallback(Action<Float01> callback)
        {
            m_AmplitudeCallbacks += callback;
        }

        public Float01 AverageValue
        {
            get { return m_AverageValue01; }
        }

        public bool IsHighInhibit
        {
            get
            {
                if (m_StrandType != StrandType.Inhibit)
                {
                    Debug.LogWarning("Trying to get inhibit value on a strand that is not an inhibit channel " +
                                     m_StrandPosition);
                }
                return m_StrandType == StrandType.Inhibit && m_IsHighInhibit;
            }
        }

        public bool IsLowInhibit
        {
            get { return m_StrandType == StrandType.Inhibit && !m_IsHighInhibit; }
        }

        float IGameStrand.Amplitude
        {
            get { return m_Amplitude; }
            set
            {
                if (Mathf.Approximately(m_Amplitude, value))
                {
                    return;
                }
                m_Amplitude = value;
                UpdateAverageValue();
            }
        }

        float IGameStrand.Threshold
        {
            get { return m_Threshold; }
            set
            {
                if (Mathf.Approximately(m_Threshold, value))
                {
                    return;
                }
                m_Threshold = value;
                UpdateAverageValue();
            }
        }

        void IGameStrand.UpdateFrequencies(int min, int max)
        {
            MinFreq = min;
            MaxFreq = max;
        }

        void IGameStrand.SetHighInhibitValue(bool isHighInhibit)
        {
            m_IsHighInhibit = isHighInhibit;
        }

        private void UpdateAverageValue()
        {
            if (Utils.Approximately(m_Threshold, 0f, .01f))
            {
                m_AverageValue01.UpdateValuePercentage(.5f);
            }
            else
            {
                m_AverageValue01.UpdateValueDirectly(m_Amplitude / m_Threshold);
            }
            if (m_AmplitudeCallbacks != null)
            {
                m_AmplitudeCallbacks(m_AverageValue01);
            }
        }

        public string FullStrandType()
        {
            return Strand == StrandType.Inhibit ? (IsHighInhibit ? "HighInhibit" : "LowInhibit") : Strand.ToString();
        }

        public override string ToString()
        {
            return string.Format("Strand: {0} Amplitude: {1} Threshold: {2} MinMax Freq: {3}-{4}", FullStrandType(), m_Amplitude, m_Threshold, MinFreq, MaxFreq);
        }
    }

    private List<GameStrand> m_GameStrands;
    private Action<GameStrand[]> m_LayoutCallbacks, m_FrequencyCallbacks;
    private Action m_AmplitudeCallback;
    private Frequencies m_LastFrequencies;
    private bool m_IsPlaying;

    [SerializeField] private bool m_AutoIncreaseMax;
    //Call back functions
    private void Awake()
    {
        EegerCommand.Instance.Register(CommandReceiver.LAYOUT, HandleLayoutCommand);
        EegerCommand.Instance.Register(CommandReceiver.FRQ, HandleFrequencyCommand);
        EegerCommand.Instance.Register(CommandReceiver.AMPLITUDE, HandleAmplitudeCommand);
        EegerCommand.Instance.Register(CommandReceiver.THRESHOLD, HandleThresholdCommand);
        EegerCommand.Instance.Register(CommandReceiver.RUN, HandleRunCommand);
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, HandlePauseCommand);
    }

    private void HandlePauseCommand(params object[] args)
    {
        m_IsPlaying = false;
        if (m_GameStrands == null)
        {
            return;
        }
        foreach (var gameStrand in m_GameStrands)
        {
            ((IGameStrand)gameStrand).Amplitude = 0f;
        }
        if (m_AmplitudeCallback != null)
        {
            m_AmplitudeCallback();
        }
    }

    private void HandleRunCommand(params object[] args)
    {
        m_IsPlaying = true;
        if (m_AmplitudeCallback != null)
        {
            m_AmplitudeCallback();
        }
        if (m_GameStrands == null)
        {
            return;
        }
        if (m_LayoutCallbacks != null)
        {
            m_LayoutCallbacks(m_GameStrands.ToArray());
        }
        if (m_FrequencyCallbacks != null)
        {
            m_FrequencyCallbacks(m_GameStrands.ToArray());
        }
    }

    private void HandleAmplitudeCommand(params object[] args)
    {
        if (m_GameStrands == null)
        {
            return;
        }
        if (args == null || args.Length < 1)
        {
            return;
        }
        var strands = args[0] as Strands;
        if (strands == null)
        {
            return;
        }
        var keys = strands.GetStrandLocations();
        foreach (int key in keys)
        {
            if (key >= 0 && key < m_GameStrands.Count)
            {
                ((IGameStrand) m_GameStrands[key]).Amplitude = strands.Get(key);
            }
        }
        if (!m_IsPlaying)
        {
            return;
        }
        if (m_AmplitudeCallback != null)
        {
            m_AmplitudeCallback();
        }
    }

    private void HandleThresholdCommand(params object[] args)
    {
        if (m_GameStrands == null)
        {
            return;
        }
        if (args == null || args.Length < 1)
        {
            return;
        }
        var strands = args[0] as Strands;
        if (strands == null)
        {
            return;
        }
        var keys = strands.GetStrandLocations();
        foreach (int key in keys)
        {
            if (key >= 0 && key < m_GameStrands.Count)
            {
                ((IGameStrand) m_GameStrands[key]).Threshold = strands.Get(key);
            }
        }
        if (!m_IsPlaying)
        {
            return;
        }
        if (m_AmplitudeCallback != null)
        {
            m_AmplitudeCallback();
        }
    }

    private void HandleLayoutCommand(params object[] args)
    {
        if (args == null || args.Length < 2)
        {
            return;
        }
        List<int> strandTypes = args[0] as List<int>;
        if (strandTypes == null)
        {
            return;
        }
        bool layoutChanged = false;
        if (m_GameStrands == null)
        {
            m_GameStrands = new List<GameStrand>();
            layoutChanged = true;
        }
        for (int i = 0; i < strandTypes.Count; i++)
        {
            var currentType = ConvertStrandIntToChannelType(strandTypes[i]);
            if (m_GameStrands.Count - 1 < i)
            {
                m_GameStrands.Add(new GameStrand(currentType, i));
                layoutChanged = true;
            }
            if (m_GameStrands[i].Strand != currentType)
            {
                m_GameStrands[i] = new GameStrand(currentType, i);
                layoutChanged = true;
            }
            m_GameStrands[i].AverageValue.AutoIncreaseMaxValue = m_AutoIncreaseMax;
        }
        UpdateFrequenciesInStrands();
        if (!m_IsPlaying)
        {
            return;
        }
        if (layoutChanged && m_LayoutCallbacks != null)
        {
            m_LayoutCallbacks(m_GameStrands.ToArray());
        }
    }

    public void LogLayout()
    {
        string layout = "";
        for (var i = 0; i < m_GameStrands.Count; i++)
        {
            GameStrand strand = m_GameStrands[i];
            layout += string.Format("Strand {0} = {1}\n",i, strand);
        }
        Debug.Log(layout);
    }

    private void HandleFrequencyCommand(params object[] args)
    {
        if (args == null || args.Length < 1)
        {
            return;
        }
        m_LastFrequencies = args[0] as Frequencies;        
        UpdateFrequenciesInStrands();
    }

    private void UpdateFrequenciesInStrands()
    {
        if (m_GameStrands == null || m_LastFrequencies == null)
        {
            return;
        }
        int minRewardFrequency = int.MaxValue;
        int maxRewardFrequency = int.MinValue;

        bool frequenciesUpdated = false;
        for (int i = 0; i < m_GameStrands.Count; i++)
        {
            var strand = m_GameStrands[i];
            if (strand == null)
            {
                continue;
            }
            int min, max;
            if (m_LastFrequencies.TryGetRange(i, out min, out max))
            {
                if (strand.MinFreq != min || strand.MaxFreq != max)
                {
                    frequenciesUpdated = true;
                    ((IGameStrand) strand).UpdateFrequencies(min, max);
                }
            }
            if (strand.Strand == StrandType.Reward)
            {
                minRewardFrequency = Mathf.Min(minRewardFrequency, strand.MinFreq);
                maxRewardFrequency = Mathf.Max(maxRewardFrequency, strand.MaxFreq);
            }
        }
        foreach (var strand in m_GameStrands)
        {
            if (strand == null || strand.Strand != StrandType.Inhibit)
                continue;
            ((IGameStrand) strand).SetHighInhibitValue(strand.MinFreq >= maxRewardFrequency);
        }
        if (frequenciesUpdated && m_FrequencyCallbacks != null)
        {
            m_FrequencyCallbacks(m_GameStrands.ToArray());
        }
        LogLayout();
    }

    public void RegisterLayoutChanged(Action<GameStrand[]> onLayoutChanged)
    {
        m_LayoutCallbacks += onLayoutChanged;
    }

    public void RegisterFrequencyChanged(Action<GameStrand[]> onLayoutChanged)
    {
        m_FrequencyCallbacks += onLayoutChanged;
    }

    public void RegisterAmplitudeChanged(Action onAmplitudeChanged)
    {
        m_AmplitudeCallback += onAmplitudeChanged;
    }
}

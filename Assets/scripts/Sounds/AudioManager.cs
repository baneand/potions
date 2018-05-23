using System.Collections.Generic;
using UnityEngine;

public enum AudioChannelType
{
    Reward = 0,
    Success = 1,
    Sound3 = 2,
    Sound4 = 3,
    Sound5 = 4,
    Sound6 = 5,
    Sound7 = 6,
    Sound8 = 7,
    Master = 8,
    Extra = 9,
    Length = 10
}

public interface IAudioLevel
{
    AudioChannelType AudioType { get; }
    void SetAudioLevel(float volume);
}

public class AudioManager : Singleton<AudioManager>
{
    private class MasterAudioLevel : IAudioLevel
    {
        public AudioChannelType AudioType
        {
            get { return AudioChannelType.Master; }
        }

        public void SetAudioLevel(float volume)
        {
            AudioListener.volume = volume;
        }
    }

    private readonly HashSet<IAudioLevel> m_Channels = new HashSet<IAudioLevel>();

    // ReSharper disable once RedundantExplicitArraySize
    private readonly float[] m_CurrentVolumes = new float[(int) AudioChannelType.Length]
    {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};

    private void Awake()
    {
        RegisterAudioLevel(new MasterAudioLevel());
    }

    public void HandleAudioEvent(Strands strands)
    {
        if (strands == null)
        {
            return;
        }
        foreach (int location in strands.GetStrandLocations())
        {
            if (location >= (int) AudioChannelType.Length || location < 0)
            {
                Debug.LogWarning("Can not handle volume for channel : " + location);
                continue;
            }
            m_CurrentVolumes[location] = strands.Get(location);
            SendCallback((AudioChannelType) location);
        }
    }

    public void RegisterAudioLevel(IAudioLevel audioLevel)
    {
        if (audioLevel == null)
        {
            return;
        }
        m_Channels.Add(audioLevel);
        SendCallback(audioLevel.AudioType);
    }

    private void SendCallback(AudioChannelType type)
    {
        float value = m_CurrentVolumes[(int) type];
        foreach (var channel in m_Channels)
        {
            if (channel != null && channel.AudioType == type)
            {
                channel.SetAudioLevel(value);
            }
        }
    }
}

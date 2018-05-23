using UnityEngine;

public class AudioSourceLevel : MonoBehaviour, IAudioLevel
{
    [SerializeField] private AudioChannelType m_AudioType;
    [SerializeField] private AudioSource m_AudioSource;

    private AudioSource Source
    {
        get
        {
            if (m_AudioSource == null)
            {
                m_AudioSource = GetComponent<AudioSource>();
            }
            return m_AudioSource;
        }
    }

    private void Awake()
    {
        AudioManager.Instance.RegisterAudioLevel(this);
    }

    public AudioChannelType AudioType
    {
        get { return m_AudioType; }
    }

    public void SetAudioLevel(float volume)
    {
        Source.volume = volume;
    }
}

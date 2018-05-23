using UnityEngine;

public class BackgroundSound : MonoBehaviour
{
    [SerializeField] private AudioSource m_BackgroundAudioSource;

    void Start()
    {
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, args => HandlePause());
        EegerCommand.Instance.Register(CommandReceiver.RUN, args => HandleRun());
    }

    private void HandleRun()
    {
        if (m_BackgroundAudioSource != null)
        {
            m_BackgroundAudioSource.UnPause();
        }
    }

    private void HandlePause()
    {
        if (m_BackgroundAudioSource != null)
        {
            m_BackgroundAudioSource.Pause();
        }
    }
}

using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] private PeriodTimeDisplay m_Display;
    [SerializeField] private CanvasGroup m_CanvasGroup;

    private void Start()
    {
        if (m_Display == null)
        {
            m_Display = GetComponentInChildren<PeriodTimeDisplay>();
        }
        if (m_CanvasGroup == null)
        {
            m_CanvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, args => Pause());
        EegerCommand.Instance.Register(CommandReceiver.RUN, args => Run());
        Pause();
    }

    private void Pause()
    {
        DisplayCanvas(false);
    }

    private void Run()
    {
        DisplayCanvas(true);
    }

    private void DisplayCanvas(bool value)
    {
        if (m_CanvasGroup != null)
        {
            m_CanvasGroup.alpha = value ? 1f : 0f;
        }
    }

    private void Update()
    {
        if (m_Display != null)
        {
            m_Display.UpdateFromPeriod(PeriodTimeManager.Instance.CurrentPeriod);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PeriodTimeDisplay : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Custom/Test Time Displays")]
    public static void TestTimeDisplays()
    {
        float time = 59f;
        string value = ConvertToTimeDisplay(time);
        if (!string.Equals("0:59", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 61f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("1:01", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 3599.99f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("59:59", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 3600;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("1:00:00", value))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 3600.99f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("1:00:00", value))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 3661f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("1:01:01", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 0f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("0:00", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 7212f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("2:00:12", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        time = 7272f;
        value = ConvertToTimeDisplay(time);
        if (!string.Equals("2:01:12", value ))
            Debug.LogError("Failure for time " + time + " value was " + value);
        Debug.Log("Finished!");
    }
#endif

    public static string ConvertToTimeDisplay(float timeInSeconds)
    {
        if (timeInSeconds >= 3600f)
        {
            int hours = (int) (timeInSeconds/3600f);
            timeInSeconds -= hours*3600f;
            int minutes = (int) (timeInSeconds/60f);
            int seconds = (int)(timeInSeconds - minutes * 60f);
            return string.Format("{0}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        else
        {
            int minutes = (int)(timeInSeconds / 60f);
            int seconds = (int) (timeInSeconds - minutes * 60f);
            return string.Format("{0}:{1:00}", minutes, seconds);
        }
    }

    public int RewardCount
    {
        get
        {
            return m_RewardCount;
        }
        set
        {
            if (m_RewardCount == value)
            {
                return;
            }
            m_RewardCount = value;
            if (m_RewardDisplay != null)
            {
                m_RewardDisplay.text = value.ToString();
            }
        }
    }

    public float TimeInSeconds
    {
        get
        {
            return m_Time;
        }
        set
        {
            m_Time = value;
            if(m_TimeDisplay != null)
            {
                m_TimeDisplay.text = ConvertToTimeDisplay(value);
            }

        }
    }

    /// <summary>
    /// this is the period number with the first period == 0
    /// </summary>
    public int PeriodNumber
    {
        get { return m_PeriodNumber; }
        set
        {
            m_PeriodNumber = value;
            if (m_PeriodNumberText != null)
            {
                m_PeriodNumberText.text = (value + 1).ToString();
            }
        }
    }

    [SerializeField] private Text m_PeriodNumberText;
    private int m_PeriodNumber;
    [SerializeField]
    private Text m_TimeDisplay;
    private float m_Time;

    [SerializeField]
    private Text m_RewardDisplay;
    private int m_RewardCount = -1;

    public void UpdateFromPeriod(Period period)
    {
        if (period == null)
        {
            return;
        }
        RewardCount = period.RewardCount;
        TimeInSeconds = period.TimeInSeconds;
        PeriodNumber = period.Position;
    }
}

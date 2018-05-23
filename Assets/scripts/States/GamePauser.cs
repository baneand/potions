using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* The GamePauser class pauses the game in response to the EEGer events RUN/PAUSE.
 * It also shows the last X number of scores
*/
public class GamePauser : MonoBehaviour
{
    //The pauseScreen that does not show when the game is unpaused
    [SerializeField]
    private CanvasGroup m_PauseScreen;

    [SerializeField] private int m_MaxItemsToShow = 3;
    [SerializeField] private bool m_StartPaused = true;
    [SerializeField] private Individual_score_Display m_TemplateDisplay;
    [SerializeField]
    private Text m_TotalTimeText;
    [SerializeField]
    private Text m_TotalRewardText;
    [SerializeField]
    private Transform[] m_PossibleParents;

    private List<Individual_score_Display> m_TimeDisplays = new List<Individual_score_Display>();
    // Use this for initialization
    private void Start()
    {
        if (m_PauseScreen == null)
        {
            m_PauseScreen = GetComponentInChildren<CanvasGroup>();
            if (m_PauseScreen == null)
            {
                m_PauseScreen = gameObject.AddComponent<CanvasGroup>();
            }
        }
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, args => Pause());
        EegerCommand.Instance.Register(CommandReceiver.RUN, args => Resume());
        //only display things if they aren't referencing a prefab
        if (m_TemplateDisplay != null && m_TemplateDisplay.hideFlags != HideFlags.HideInHierarchy)
        {
            m_TemplateDisplay.gameObject.SetActive(false);
        }
        if (m_StartPaused)
        {
            Pause();
        }
    }

    //Pause the whole game 
    public void Pause ()
    {
        //Time.timeScale = 0.0f;
        m_PauseScreen.alpha = 1f;
        m_PauseScreen.interactable = m_PauseScreen.blocksRaycasts = true;
        if (m_TotalTimeText != null)
        {
            m_TotalTimeText.text = PeriodTimeDisplay.ConvertToTimeDisplay(PeriodTimeManager.Instance.TotalTime);
        }
        if (m_TotalRewardText != null)
        {
            m_TotalRewardText.text = PeriodTimeManager.Instance.TotalRewards.ToString();
        }
        var periods = PeriodTimeManager.Instance.GetLastPeriods(m_MaxItemsToShow * m_PossibleParents.Length);
        if (periods == null || m_TemplateDisplay == null)
        {
            return;
        }
        int sub_value = 0;
        int total_Reward_num = 0;
        for (int i = 0; i < periods.Length; i++)
        {
            if (m_TimeDisplays.Count <= i)
            {
                m_TimeDisplays.Add(m_TemplateDisplay.Duplicate());
            }
            var item = m_TimeDisplays[i];
            var period = periods[i];
            item.UpdateFromPeriod(period);
            item.RewardCount -= sub_value;
            sub_value += item.RewardCount;
            total_Reward_num += item.RewardCount;
            item.transform.SetParent(m_PossibleParents[i / m_MaxItemsToShow]);
            m_PossibleParents[i/ m_MaxItemsToShow].gameObject.SetActive(true);
        }
        m_TotalRewardText.text = total_Reward_num.ToString();
    }
	
	// And resume gameplay
	public void Resume ()
    {
        //Time.timeScale = 1.0f;
        m_PauseScreen.alpha = 0f;
        m_PauseScreen.interactable = m_PauseScreen.blocksRaycasts = false;
    }
}

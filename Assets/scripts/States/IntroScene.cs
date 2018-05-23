using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    [SerializeField]
    private float m_ProgressTime;

    [SerializeField]
    private float m_FadeOutTime = 5;

    [SerializeField] private RectTransform m_ProgressRectTransform;
    [SerializeField] private CanvasGroup m_CanvasGroup;
	// Use this for initialization
	IEnumerator Start ()
	{
	    float totalTime = 0f;
	    while (totalTime < m_ProgressTime)
	    {
	        totalTime += Time.deltaTime;
            m_ProgressRectTransform.anchorMax = new Vector2(totalTime/m_ProgressTime, 1f);
            yield return null;
	    }
        SwitchScenes();
	}

    //unity tends to not like loading scene from coroutines so we call it in a regular function
    private void SwitchScenes()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        StartCoroutine(CoFadeOutIntro());
    }

    IEnumerator CoFadeOutIntro()
    {
        if (m_CanvasGroup == null)
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        float finishTime = Time.realtimeSinceStartup + m_FadeOutTime;
        while (finishTime > Time.realtimeSinceStartup)
        {
            m_CanvasGroup.alpha = (finishTime - Time.realtimeSinceStartup)/ m_FadeOutTime;
            yield return null;
        }
        //unload the current scene
        SceneManager.UnloadSceneAsync(0);
    }
}

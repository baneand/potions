using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeTracker : MonoBehaviour
{
    public static TimeTracker Instance;

    // Use this for initialization
    public static Slider Period1ScoreSlider;
    public static Text Period1ScoreText;

    //public static Slider recordedText1;

    public static Slider Period1TimeSlider;
    public static Text Period1TimeText;


    public float tempTime;
    public float score;
    public bool recordTime = false;
    public GameObject myCanvas;
    void Awake()
    {
        myCanvas = GameObject.Find("UI Canvas");
        if(myCanvas == null)
        {
            Debug.LogError("Canvas named \"UI Canvas\" was not found");
        }
        var periodScoreSliderTransform = myCanvas.transform.Find("Period1ScoreSlider");
        if (periodScoreSliderTransform != null)
        {
            Period1ScoreSlider = periodScoreSliderTransform.GetComponent<Slider>();
            if (Period1ScoreSlider != null)
            {
                var fillArea = Period1ScoreSlider.transform.Find("Fill Area");
                if (fillArea != null)
                {
                    var textTransform = fillArea.transform.Find("Text");
                    if (textTransform != null)
                    {
                        Period1ScoreText = textTransform.GetComponent<Text>();
                    }
                }
            }
        }

        periodScoreSliderTransform = myCanvas.transform.Find("Period1TimeSlider");
        if (periodScoreSliderTransform != null)
        {
            Period1TimeSlider = periodScoreSliderTransform.GetComponent<Slider>();
            if (Period1TimeSlider != null)
            {
                var fillArea = Period1TimeSlider.transform.Find("Fill Area");
                if (fillArea != null)
                {
                    var textTransform = fillArea.transform.Find("Text");
                    if (textTransform != null)
                    {
                        Period1TimeText = textTransform.GetComponent<Text>();
                    }
                }
            }
        }
        if (Instance)
        {
            DestroyObject(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }


        //    DestroyObject(gameObject);


    }

    void Start()
    {

        score = 0;
        //updateScore();
    }

    // Update is called once per frame
    void Update()
    {
        //updateScore();
        UpdateTimer();
        if (Input.GetKeyDown("k"))
        {
            addScore();
        }
        if (Input.GetKeyDown("t"))
        {
            StartTimer();
            //score = 0;
        }
        if (Input.GetKeyUp("t"))
        {
            //StopTimer();
        }

    }
    public void addScore()
    {
        score += 0.1f;

    }

    public void UpdateTimer()
    {
        if(Period1ScoreSlider != null)
        {
            Period1ScoreSlider.value = score * 0.01f;
        }

        if (Period1ScoreText != null)
        {
            Period1ScoreText.text = "Period 1 Score: " + score;
        }
        if(Period1TimeSlider != null)
        {
            Period1TimeSlider.value = tempTime * 0.01f;
        }
        //recordedText1.text = "1st attempt: " + tempTime;
    }
    public void StartTimer()
    {
        recordTime = true;
        tempTime = Time.timeSinceLevelLoad;
        Period1TimeText.text = "Period 1 Time: " + Mathf.Floor(Time.timeSinceLevelLoad);

        SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
    }

}
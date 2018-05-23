using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPauseAction : MonoBehaviour,IOptionSection {

    Animator ani;
    int RewardCount; // Used for the Graph
    int localRewardCount;// Used for Rotations.
    int RotationStringMarker;

    int CountPerRotation = 10;
    
    float[] time = new float[4]; // used for Zoom animation
    bool ZoomAllowed = false;
    bool isPaused;
    bool FirstRotation = true;

    string[] RotationString = new string[4] {"Rotation1","Rotation2","Rotation3","Rotation4" };
    string[] BirdsEye = new string[4] { "BirdsEye2", "BirdsEye3", "BirdsEye4", "BirdsEye1" };

    public string m_DefName;
    public string m_HelpString;
    public int m_GameDefOrder;



    int m_MaxValueForReward;
    

	// Use this for initialization
	void Start () {

        RewardCount = 0;
        localRewardCount = 0;
        RotationStringMarker = 0;
        isPaused = true;
        ani = GetComponent<Animator>();

 

        EegerCommand.Instance.Register(CommandReceiver.REWARD, HandleReward);           
        EegerCommand.Instance.Register(CommandReceiver.PAUSE, HandlePause);             
        EegerCommand.Instance.Register(CommandReceiver.RUN, HandleRun);
        OptionsManager.Instance.RegisterOptionChanged(m_DefName, OnOptionSet);

    }

    private void HandleRun(params object[] args)
    {
        ZoomAllowed = true;

      
            isPaused = true;
            if (time[RotationStringMarker] > 0)
                time[RotationStringMarker] -= 0.3f * Time.deltaTime;
      //      ani.Play(BirdsEye[RotationStringMarker], 0, time[RotationStringMarker]);
        
    }

    private void HandlePause(params object[] args)
    {
        if (ZoomAllowed)
        {
            isPaused = false;
            if (time[RotationStringMarker] < 1)
                time[RotationStringMarker] += 0.1f * Time.deltaTime;

       //     ani.Play(BirdsEye[RotationStringMarker], 0, time[RotationStringMarker]);
        }

    }

    private void HandleReward(params object[] args)
    {

        RewardCount++;
        localRewardCount++;

        if (localRewardCount > CountPerRotation )
        {
            FirstRotation = false;
            localRewardCount = 0;
            RotationStringMarker++;


            if (RotationStringMarker > 3) RotationStringMarker = 0;

            time[RotationStringMarker] = 0.001f;
            ani.Play(RotationString[RotationStringMarker]);
            ZoomAllowed = false;
 
        }


    }

    private void OnOptionSet(string s)
    {
        int optionValue;
        if (string.IsNullOrEmpty(s) || !int.TryParse(s, out optionValue))
        {
            Debug.LogWarning("Could not parse Camera reward option to an int " + s);
            return;
        }
        if (optionValue == CountPerRotation)
        {
            return;
        }
        Debug.Log("Setting Fish reward count to " + optionValue);
        //subtract the remaining rewards before a bonus and then add the new value to set the remaining rewards
        CountPerRotation = optionValue;

    }
 
    // Update is called once per frame
    void Update () {


        if (ani.GetCurrentAnimatorStateInfo(0).IsName("Dummy") == true)  ZoomAllowed = true;

        if (isPaused && ZoomAllowed)
        {
            if (time[RotationStringMarker] > 0)
                time[RotationStringMarker] -= 0.3f * Time.deltaTime;
        }
        else if(ZoomAllowed)
        {
            if (time[RotationStringMarker] < 1)
                time[RotationStringMarker] += 0.1f * Time.deltaTime;
        }

        if (ZoomAllowed)
        {
            if(FirstRotation)
                ani.Play("BirdsEye1", 0, time[3]);
            else
                ani.Play(BirdsEye[RotationStringMarker], 0, time[RotationStringMarker]);
        }

    }



    #region Gamedef Option/Def

    public string GamedefName
    {
        get { return m_DefName; }
    }

    public string OptionLayout
    {
        get { return string.Format("I,1,{0},{1},{2}", m_MaxValueForReward, m_GameDefOrder, CountPerRotation); }
    }

    public string HelpString
    {
        get { return m_HelpString; }
    }


    #endregion
}

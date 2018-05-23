using UnityEngine;
using System.Collections;

/* This SunlightController is applied to a procedural skybox by changing the exposure of the script, resembling the adding and subtracting of sunlight
*/
public class SunlightController : ShrinkingRewardSubscriber
{
    //The shader parameter corresponding to sunlight
    private const string sunlightParameterName = "_Exposure";
    //The starting amount of sunlight
    private const float startSunlightMultiplier = 0.5f;

    //How much sunlight is present
    private float currentSunlight;

    //How much sunlight increases/decreases
    public float sunlightChangeValue;

    //How much sunlight is the maximum
    public float maxSunlight;


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        currentSunlight = startSunlightMultiplier * maxSunlight;
        UpdateSunlightChange();
    }

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();
        currentSunlight = startSunlightMultiplier * maxSunlight;
        UpdateSunlightChange();
    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        //Add towards a sunnier day
        currentSunlight += sunlightChangeValue;

        if (currentSunlight > maxSunlight)
            currentSunlight = maxSunlight;

        UpdateSunlightChange();
    }

    public override void NegateReward()
    {
        base.NegateReward();
        if (!Subscribed)
            return;

        //Subtract towards a darker day
        currentSunlight -= sunlightChangeValue;

        if (currentSunlight < 0.0f)
            currentSunlight = 0.0f;

        UpdateSunlightChange();
    }

    //This function takes a changed value and represents it via a color that it changes the sky with
    void UpdateSunlightChange()
    {
        RenderSettings.skybox.SetFloat(sunlightParameterName, currentSunlight);
        DynamicGI.UpdateEnvironment();
    }
}

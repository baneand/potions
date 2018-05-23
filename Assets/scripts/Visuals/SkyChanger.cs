using UnityEngine;
using System.Collections;

/* The SkyChanger changes the color of the sky based on the data values that it receives from its controller.
*/
public class SkyChanger : ShrinkingRewardSubscriber
{

    public Color positiveColor;
    public Color negativeColor;
    private const float startValue = 0.5f;

    private Color currentColor;
    private float currentValue;

    public float incrementValue = 0.1f;


    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        currentValue = startValue;
        UpdateColorChange();
    }

    public override void ResetSubscriber()
    {
        base.ResetSubscriber();
        currentValue = startValue;
        UpdateColorChange();
    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        //Add towards a positive color
        currentValue += incrementValue;

        if (currentValue > 1.0f)
            currentValue = 1.0f;

        UpdateColorChange();
    }

    public override void NegateReward()
    {
        base.NegateReward();

        //Subtract towards a negative color
        currentValue -= incrementValue;

        if (currentValue < 0.0f)
            currentValue = 0.0f;

        UpdateColorChange();
    }

    //This function takes a changed value and represents it via a color that it changes the sky with
    void UpdateColorChange()
    {
        currentColor = Color.Lerp(negativeColor, positiveColor, currentValue);
        RenderSettings.skybox.SetColor("_SkyTint", currentColor);
        DynamicGI.UpdateEnvironment();
    }
	
}

using UnityEngine;
using System.Collections;

/* The ParticleColorChanger takes a particle and interpolates its color between set points 
*/

public class ParticleColorChanger : ShrinkingRewardSubscriber {

    //When changing shader parameters, this is the parameter for the color of the tint
    private const string CloudTintParameter = "_TintColor";

    //The particle system that controls the clouds
    public ParticleSystemRenderer particleRender;

    //The colors to interpolate between
    public Color positiveColor;
    public Color negativeColor;

    //How quickly to change color on the clouds
    public float colorChangeRate;

    private float currentColorValue = 0.9f;

    //Used in case of cloud cover - set the value if it exists
    public Rainfall rainFall;

    protected override void Start()
    {
        base.Start();
        UpdateParticleColor();
    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        currentColorValue += colorChangeRate;

        if (currentColorValue > 1.0f)
            currentColorValue = 1.0f;

        UpdateParticleColor();
    }

    public override void NegateReward()
    {
        base.NegateReward();

        if (!Subscribed)
            return;

        currentColorValue -= colorChangeRate;

        if (currentColorValue < 0.0f)
            currentColorValue = 0.0f;

        UpdateParticleColor();
    }

    private void UpdateParticleColor()
    {
        particleRender.material.SetColor(CloudTintParameter, Color.Lerp(negativeColor, positiveColor, currentColorValue));

        if (rainFall)
            rainFall.SetCloudColorValue(currentColorValue);
    }
}

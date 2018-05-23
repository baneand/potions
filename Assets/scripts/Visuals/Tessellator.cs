using UnityEngine;
using System.Collections;

/* This object applies a displacement to a tessellated object using the FixedTessellation shader.
*/
public class Tessellator : ShrinkingRewardSubscriber {

    //Rate of displacing the tessellated vertices
    public float displacementChangeRate;

    //Rate of smoothly flowing towards a set displacement
    public float displacementFlowRate;

    //A set min/max value for  how much to tessellate the object
    public float maxDisplacement;
    public float minDisplacement;

    //How much the object is displaced over time - always interpolates toward a specific target
    public float currentTargetDisplacement = 0;
    public float currentDisplacement = 0;

    //The renderer of the object that the Tessellator applies displacement to
    public MeshRenderer meshRender;

    //On reward, increase displacement (if not reverse)
    protected override void ActivateReward()
    {
        base.ActivateReward();

        if (isReverse)
            DecreaseDisplacement();
        else
            IncreaseDisplacement();
    }

    private void DecreaseDisplacement()
    {
        currentTargetDisplacement -= displacementChangeRate;
        if (currentTargetDisplacement < minDisplacement)
            currentTargetDisplacement = minDisplacement;
    }

    private void IncreaseDisplacement()
    {
        currentTargetDisplacement += displacementChangeRate;
        if (currentTargetDisplacement > maxDisplacement)
            currentTargetDisplacement = maxDisplacement;
    }

    //Sets displacement value in the tessellation material upon changing the value
    private void UpdateDisplacement()
    {
        meshRender.material.SetFloat("_Displacement", currentDisplacement);
    }

    private void Update()
    {
        //Interpolate displacement towards target displacement over time
        currentDisplacement = Mathf.Lerp(currentDisplacement, currentTargetDisplacement, Time.deltaTime * displacementFlowRate);
        UpdateDisplacement();
    }

    //Cancels all displacement on the object
    public override void ResetSubscriber()
    {
        base.ResetSubscriber();

        currentTargetDisplacement = minDisplacement;

        currentDisplacement = currentTargetDisplacement;

        UpdateDisplacement();
    }

    //On negation, decrease displacement (if not reversed)
    public override void NegateReward()
    {
        base.NegateReward();

        if (isReverse)
            IncreaseDisplacement();
        else
            DecreaseDisplacement();
    }

    //On reversal, restart the displacement values
    public override void ToggleReverse()
    {
        base.ToggleReverse();

        if (!Subscribed)
            return;

        if (isReverse)
            currentTargetDisplacement = maxDisplacement;
        else
            currentTargetDisplacement = minDisplacement;

        currentDisplacement = currentTargetDisplacement;

        UpdateDisplacement();
    }
}

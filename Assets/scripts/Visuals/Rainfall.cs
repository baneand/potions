using UnityEngine;
using System.Collections;

/* Rainfall only exists when the clouds are dark enough (below a certain value) and there are enough clouds in the sky. This can be regulated in the main scene.
*/

public class Rainfall : MonoBehaviour {

    //This system controls the rain that falls from the sky
    public ParticleSystem rainParticleSystem;

    //How much cloud cover is there for rain to fall?
    public float maxCloudCoverValue;

    //How dark does the color have to be for rain to fall?
    public float minCloudColorValue;

    public float currentCloudColorValue = 0;

    public float currentCloudCoverValue = 0;

    //The sound of rain; turns on and off when rain starts and stops.
    public AudioSource rainSound;

    //This is called to turn rain on or off at any point the requirements are met (or not met)
    private void CheckRainfallRequirements()
    {
        if (currentCloudColorValue < minCloudColorValue && currentCloudCoverValue > maxCloudCoverValue)
            TurnOnRain();
        else
            TurnOffRain();
    }

    //Sets the current value representing the color of the clouds in the sky
    public void SetCloudColorValue(float value)
    {
        currentCloudColorValue = value;
        CheckRainfallRequirements();
    }

    //Sets the current value representing the amount of clouds in the sky
    public void SetCloudCoverValue(float value)
    {
        currentCloudCoverValue = value;
        CheckRainfallRequirements();
    }

    //This turns off the rain particle system
	private void TurnOffRain()
    {
        var emission = rainParticleSystem.emission;
        emission.enabled = false;
        rainSound.enabled = false;
    }

    //This turns on the rain particle system
    private void TurnOnRain()
    {
        var emission = rainParticleSystem.emission;
        emission.enabled = true;
        rainSound.enabled = true;
    }
}

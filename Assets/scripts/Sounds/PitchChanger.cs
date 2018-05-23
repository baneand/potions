using UnityEngine;

/* The PitchChanger changes the pitch of its sound based on the data values that it receives from its controller.
*/
public class PitchChanger : MessageSubscriber
{

    //Accessing the audio on the object that this script is attached to
    public AudioSource audioToChange;

    private float currentPitch = 1.0f;

    //Lower and raise the pitch
    public KeyCode negativeKey;
    public KeyCode positiveKey;

    //How much to increment on each key press
    public float incrementValue = 0.1f;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        UpdatePitchChange();
    }

    //
    void UpdatePitchChange()
    {
        audioToChange.pitch = currentPitch;
    }

    public override void UpdateData(float value, EegerMessageType valueType)
    {
        base.UpdateData(value, valueType);

        if (thresholdValue == 0.0f)
            return;

        currentPitch = amplitudeValue / thresholdValue;
        UpdatePitchChange();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(negativeKey))
        {
            currentPitch -= incrementValue;
            UpdatePitchChange();
        }
        else if (Input.GetKey(positiveKey))
        {
            currentPitch += incrementValue;
            UpdatePitchChange();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//Toggles a button color from normal to highlighted color for knowing when something's picked or not
public class ButtonColorChanger : MonoBehaviour {

    //Gets colors to toggle to from the attached button's colors
    private Color normalColor;
    private Color pressedColor;

    //Bool for switching between normal and highlighted colors
    private bool isNormal = true;

    //The button  to toggle colors
    public Button buttonToChange;

    //Is there a list of buttons reliant on this button? For de-selecting related buttons
    public List<ButtonColorChanger> subButtons;

    //In the case of reverse buttons or other non-main buttons, used to validate whether the button color should change
    //Based on the validity of a subscriber
    public RewardSubscriber subscriber;

	// Get the button's color data here
	public void Start ()
    {
	    if(buttonToChange != null && buttonToChange.transition == Selectable.Transition.ColorTint)
        {
           normalColor = buttonToChange.colors.normalColor;
           pressedColor = buttonToChange.colors.pressedColor;
        }
	}
	
	// Simply toggles the color of the button from normal to highlighted color
	public void ToggleColor ()
    {
        //Exits early if there is a subscriber attached and it is not subscribed
        if (subscriber != null && !subscriber.Subscribed)
            return;

        if (isNormal)
            ChangeNormalColor(pressedColor);
        else
        {
            //Unpress all subButtons in list if there are any
            foreach (ButtonColorChanger subButton in subButtons)
            {
                subButton.ChangeNormalColor(normalColor);
            }
            ChangeNormalColor(normalColor);
        }
            
	}

    //This changes the normal (and highlighted) color of the button to change
    private void ChangeNormalColor(Color changeColor)
    {
        ColorBlock colors = buttonToChange.colors;
        colors.normalColor = changeColor;
        colors.highlightedColor = changeColor;
        buttonToChange.colors = colors;

        //Toggle isNormal boolean
        isNormal = !isNormal;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/* When a cursor hovers over a button, the button may use this tooltip to display useful information
*/
public class TooltipManager : MonoBehaviour
{
    //How much offset the tooltip appears from the cursor when it hovers over a button
    public Vector2 offsetFromCursor;

    //How quickly the tooltip fades in over time
    public float timeToFadeInTooltip;

    //The border of the tooltip. Is separate from the text.
    public Image tooltipBorder;

    //The text of the tooltip. Is separate from the border.
    public Text tooltipText;

    //The UI transform of the tooltip. Used for moving button to specific locations
    public RectTransform tooltipTransform;

    // Turns on the tooltip and displays the approrpiate text
    public void DisplayTooltip(string message)
    {
        //Don't repeat the action more than once
        if (tooltipBorder.enabled)
            return;

        tooltipBorder.enabled = true;
        tooltipText.enabled = true;

        //Set position of the tooltip on the screen to the mouse position plus a specified offset.
        tooltipTransform.position = new Vector3(Input.mousePosition.x + offsetFromCursor.x, Input.mousePosition.y + offsetFromCursor.y, 0);

        tooltipText.text = message;

        //Set the alpha to 0 and then cross fade over a specific time
        tooltipBorder.CrossFadeAlpha(0.0f, 0, true);
        tooltipText.CrossFadeAlpha(0.0f, 0, true);

        tooltipBorder.CrossFadeAlpha(1.0f, timeToFadeInTooltip, true);
        tooltipText.CrossFadeAlpha(1.0f, 0, true);
    
    }

    public void HideTooltip()
    {
        tooltipBorder.enabled = false;
        tooltipText.enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Fade in the object over time
	    if(tooltipBorder.enabled)
        {

        }
	}
}

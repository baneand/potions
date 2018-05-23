using UnityEngine;
using System.Collections.Generic;

/* This takes a series of background assets and repeats them along a specific offset for a specific number of times.
    Used specifically for the Infinite Roll application.
*/

public class BackgroundAssetRepeater : MonoBehaviour {

    //How many times to repeat the asset construction
    public int timesToRepeat;

    //What specific assets to duplicate
    public List<GameObject> assetsToRepeat;

    //How far to duplicate the group of assets each time
    public Vector3 repeatOffset;

	// Use this for initialization
	void Start ()
    {
        RepeatConstruction(timesToRepeat, Vector3.zero);
	}
	
    private void RepeatConstruction(int numberOfTimes, Vector3 currentOffset)
    {
        if (numberOfTimes == 0)
            return;

        //Add repeat offset to current position
        currentOffset += repeatOffset;

        //Create a new set of assets for each repeat
        foreach(GameObject asset in assetsToRepeat)
        {
            GameObject repeatedAsset = Instantiate(asset);
            repeatedAsset.transform.position = asset.transform.position + currentOffset;
            repeatedAsset.transform.parent = transform;
        }

        //Recursively call the number of times down to 0
        RepeatConstruction(numberOfTimes - 1, currentOffset);
    }
	
}

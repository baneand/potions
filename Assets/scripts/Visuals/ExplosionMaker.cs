using UnityEngine;

/* This class creates explosions in response to reward events.
*/
public class ExplosionMaker : RewardSubscriber {

    //The prefab that contains the explosion data
    public GameObject explosionPrefab;

    //Target of what object to follow
    public GameObject objectTarget;

    private void Update()
    {
        if (objectTarget == null)
            return;

        //Consistently follow an object target
        transform.position = objectTarget.transform.position;
    }

    //In which rewards come in the form of an explosion
    protected override void ActivateReward()
    {
        if (explosionPrefab == null)
            return;

        //Make an explosion and attach it to this object
        GameObject explosion = Instantiate(explosionPrefab) as GameObject;
        explosion.transform.parent = transform;
        explosion.transform.localPosition = Vector3.zero;
    }
}

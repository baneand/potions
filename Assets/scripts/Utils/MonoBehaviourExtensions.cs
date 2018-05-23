using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static T Duplicate<T>(this T objectToDuplicate, bool autoActivate = true, bool worldStays = false) where T : Component
    {
        if (objectToDuplicate == null)
        {
            return null;
        }

        var returnVal = Object.Instantiate(objectToDuplicate);
        returnVal.transform.SetParent(objectToDuplicate.transform.parent, worldStays);
        if (autoActivate)
        {
            returnVal.gameObject.SetActive(true);
        }
        return returnVal;
    }
}

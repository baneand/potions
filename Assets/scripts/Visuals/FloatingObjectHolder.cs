using UnityEngine;
using System.Collections.Generic;

/*This object holds and floats a series of floating objects upon receiving rewards,
    and reverses floatation on objects upon negation.
*/
public class FloatingObjectHolder : ShrinkingRewardSubscriber
{
    [SerializeField]
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private int m_NumObjectsToFloat = 2;
    [SerializeField]
    private Vector3 m_Offset;
    [SerializeField]
    private FloatingObject m_TemplateFloatingObject;

    //Objects that are currently floating
    private List<FloatingObject> m_FloatingObjects;

    //Objects that haven't floated yet
    private List<FloatingObject> m_NotFloatingObjects;

    protected override void Start()
    {
        m_FloatingObjects = new List<FloatingObject>();
        m_NotFloatingObjects = new List<FloatingObject>();
        base.Start();
        if (m_TemplateFloatingObject == null)
        {
            Debug.LogError("Missing template object can not create floating objects");
            return;
        }
        m_NotFloatingObjects.Add(m_TemplateFloatingObject);
        for (int i = 1; i < m_NumObjectsToFloat; i++)
        {
            var copy = m_TemplateFloatingObject.Duplicate();
            copy.transform.position = m_TemplateFloatingObject.transform.position + i*m_Offset;
            m_NotFloatingObjects.Add(copy);
        }
    }

    protected override void ActivateReward()
    {
        base.ActivateReward();

        if(m_NotFloatingObjects.Count > 0)
        {
            //Float object and take it out of the list of objects to float
            FloatingObject floater = m_NotFloatingObjects[Random.Range(0, m_NotFloatingObjects.Count)];
            floater.Float();

            m_NotFloatingObjects.Remove(floater);
            m_FloatingObjects.Add(floater);
        }
    }

    public override void NegateReward()
    {
        base.NegateReward();
        if (m_FloatingObjects.Count > 0)
        {
            //Float object and take it out of the list of objects to float
            FloatingObject floater = m_FloatingObjects[Random.Range(0, m_FloatingObjects.Count)];
            floater.Defloat();

            m_FloatingObjects.Remove(floater);
            m_NotFloatingObjects.Add(floater);
        }
    }
}

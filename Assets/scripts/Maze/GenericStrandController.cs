using System.Collections.Generic;
using UnityEngine;

public class GenericStrandController : EegerStrandController
{
    [SerializeField] private GenericStrandIndicator[] m_ModelMazeStrandIndicators;

    private readonly List<GenericStrandIndicator> m_CreatedIndicators = new List<GenericStrandIndicator>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var model in m_ModelMazeStrandIndicators)
        {
            if (model != null)
            {
                model.gameObject.SetActive(false);
            }
        }
    }

    protected override IStrandIndicator CreateIndicatorForType(StrandsManager.GameStrand gameStrand)
    {
        foreach (var indicator in m_ModelMazeStrandIndicators)
        {
            if (indicator.IndicatorStrandType == gameStrand.Strand)
            {
                return DuplicateIndicator(indicator);
            }
        }
        return null;
    }

    private GenericStrandIndicator DuplicateIndicator(GenericStrandIndicator model)
    {
        //remove any items that have already been deleted
        for (int i = 0; i < m_CreatedIndicators.Count; i++)
        {
            if (m_CreatedIndicators[i] == null)
            {
                m_CreatedIndicators.RemoveAt(i);
                i--;
            }
        }
        //create with required spacing
        var copy = Instantiate(model);
        copy.gameObject.SetActive(true);
        copy.transform.SetParent(model.transform.parent, false);
        m_CreatedIndicators.Add(copy);
        return copy;
    }

}

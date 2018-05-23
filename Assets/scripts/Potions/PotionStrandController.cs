#if FALSE
using UnityEngine;
public class PotionStrandController : EegerStrandController
{
    [SerializeField] private Transform[] m_StrandPositions;
    [SerializeField] private PotionsVialRewardController[] m_PossibleControllers;

    protected override void Awake()
    {
        base.Awake();
        foreach (var possibleController in m_PossibleControllers)
        {
            if (possibleController != null)
            {
                possibleController.gameObject.SetActive(false);
            }
        }
    }

    protected override IStrandIndicator CreateIndicatorForType(StrandsManager.GameStrand gameStrand)
    {
        if (m_PossibleControllers == null)
            return null;
        foreach (var controller in m_PossibleControllers)
        {
            if (controller != null && controller.IndicatorStrandType == gameStrand.Strand)
            {
                var returnVal = controller.Duplicate();
                returnVal.transform.SetParent(FindFirstValidParent());
                returnVal.transform.localPosition = Vector3.zero;
                returnVal.transform.localRotation = Quaternion.identity;
                return returnVal;
            }
        }
        return null;
    }

    private Transform FindFirstValidParent()
    {
        if (m_StrandPositions == null)
        {
            return transform;
        }
        foreach (Transform position in m_StrandPositions)
        {
            if (position.childCount == 0)
            {
                return position;
            }
        }
        return transform;
    }
}
#endif
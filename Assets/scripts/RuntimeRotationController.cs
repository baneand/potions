using System;
using UnityEngine;

public class RuntimeRotationController : MonoBehaviour
{
    [Serializable]
    public class RotationCombination
    {
        public Transform ObjectToRotate;
        public Vector3 Rotation;
        public bool UseWorldSpace;
    }

    [SerializeField] private RotationCombination[] m_RotationCombinations;

    private void Awake()
    {
        foreach (var combination in m_RotationCombinations)
        {
            if (combination == null || combination.ObjectToRotate == null)
            {
                continue;
            }
            if (combination.UseWorldSpace)
            {
                combination.ObjectToRotate.rotation = Quaternion.Euler(combination.Rotation);
            }
            else
            {
                combination.ObjectToRotate.localRotation = Quaternion.Euler(combination.Rotation);
            }
        }
    }
}

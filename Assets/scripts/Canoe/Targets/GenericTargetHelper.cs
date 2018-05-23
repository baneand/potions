using UnityEngine;

public interface ITarget
{
    Transform Target { get; set; }
}

namespace Eeger.Canoe
{
    public class GenericTargetHelper : MonoBehaviour
    {
        [SerializeField]
        private TargetManager.TargetType m_Type;
        [SerializeField]
        private MonoBehaviour m_TargetBehaviour;

        void Start()
        {
            HandleAssignTarget(TargetManager.Instance.GetTarget(m_Type));
        }

        protected void HandleAssignTarget(Transform target)
        {
            var iTarget = m_TargetBehaviour as ITarget;
            if (iTarget != null)
            {
                iTarget.Target = target;
            }
        }
    }
}
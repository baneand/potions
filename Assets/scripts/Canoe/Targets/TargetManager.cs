using System.Collections.Generic;
using UnityEngine;

namespace Eeger.Canoe
{
    public class TargetManager
    {
        private const float MinDistance = 30f;
        private const float MaxDistance = 50f;

        public static TargetManager Instance
        {
            get { return s_Instance ?? (s_Instance = new TargetManager()); }
        }

        private static TargetManager s_Instance;
        public enum TargetType
        {
            Character,
            LookPoint,
            Destination
        }

        private readonly Dictionary<TargetType, Transform> m_Targets = new Dictionary<TargetType, Transform>();

        public void RegisterTarget(TargetType type, Transform go)
        {
            m_Targets[type] = go;
        }

        public Transform GetTarget(TargetType type)
        {
            Transform foundObject;
            return m_Targets.TryGetValue(type, out foundObject) ? foundObject : null;
        }

        public void SetNextDestination()
        {
            var destintaiton = GetTarget(TargetType.Destination);
            var currPosition = destintaiton.position;
            currPosition += new Vector3(Random.Range(MinDistance, MaxDistance), 0f, Random.Range(MinDistance, MaxDistance));
            destintaiton.position = currPosition;
        }
    }
}
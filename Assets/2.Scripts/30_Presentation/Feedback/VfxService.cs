using UnityEngine;

namespace Hourbound.Presentation.Feedback
{
    // 간단한 VFX 서비스(프리팹 생성)
    public sealed class VfxService : MonoBehaviour
    {
        public void Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return;
            Instantiate(prefab, position, rotation);
        }
    }
}
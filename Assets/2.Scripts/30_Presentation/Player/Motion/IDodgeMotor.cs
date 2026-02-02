using UnityEngine;

namespace Hourbound.Presentation.Player.Motion
{
    public interface IDodgeMotor
    {
        // 대시를 distance만큼 duration동안 수행한다.
        void Dodge(Vector3 worldDirNormalized, float distance, float duration);
        bool IsDodging { get; }
    }
}

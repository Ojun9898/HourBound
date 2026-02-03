using UnityEngine;

namespace Hourbound.Presentation.Player
{
    public sealed class MoveInputCache : MonoBehaviour
    {
        public Vector2 Value { get; private set; }
        public void Set(Vector2 v) => Value = Vector2.ClampMagnitude(v, 1f);
    }
}
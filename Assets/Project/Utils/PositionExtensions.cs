using UnityEngine;

namespace BPC.Utils
{
    public static class PositionExtensions
    {
        public static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }
    }
}
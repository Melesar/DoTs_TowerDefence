using UnityEngine;

namespace DoTs.Utilites
{
    public static class VectorExtensions
    {
        public static Vector2 AsVector2(this Vector3 vec)
        {
            return vec;
        }
        
        public static Vector3 AsVector3(this Vector2 vec)
        {
            return vec;
        }
    }
}
using System;

namespace DoTs.Graphics
{
    public struct AnimationTypeWrapper : IEquatable<AnimationTypeWrapper>
    {
        public readonly AnimationEntityType type;

        public AnimationTypeWrapper(AnimationEntityType type)
        {
            this.type = type;
        }

        public bool Equals(AnimationTypeWrapper other)
        {
            return type == other.type;
        }

        public override int GetHashCode()
        {
            return (int) type;
        }
    }
    
    public enum AnimationEntityType
    {
        Enemy,
        TurretExplosion
    }
}
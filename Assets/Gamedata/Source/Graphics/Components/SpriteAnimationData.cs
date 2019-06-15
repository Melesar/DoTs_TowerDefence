using Unity.Entities;

namespace DoTs.Graphics
{
    public struct SpriteAnimationData : IComponentData
    {
        public int currentFrame;
        public int maxFrame;
        
        public float frameTime;
        public float currentFrameTime;

        public AnimationEntityType entityType;
    }
}
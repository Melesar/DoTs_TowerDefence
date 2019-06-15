using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DoTs.Graphics
{
    [CreateAssetMenu(menuName = "DoTs/Animations/Animation sequence")]
    public class AnimationSequence : ScriptableObject
    {
        [SerializeField]
        private AnimationEntityType _type;
        [SerializeField]
        private UnityEngine.Sprite[] _frames;
        [SerializeField]
        private float _frameTime;

        public AnimationEntityType Type => _type;
        public float FrameTime => _frameTime;
        public int TotalFrames => _frames.Length;

        public float4[] GetSequenceData()
        {
            var uvs = new float4[_frames.Length];
            for (int i = 0; i < _frames.Length; i++)
            {
                var spriteTexture = _frames[i].texture;
                var invTextureSizeX = 1f / spriteTexture.width;
                var invTextureSizeY = 1f / spriteTexture.height;
                
                var spriteRect = _frames[i].rect;
                var sizeX = spriteRect.size.x * invTextureSizeX;
                var sizeY = spriteRect.size.y * invTextureSizeY;
                var offsetX = spriteRect.min.x * invTextureSizeX;
                var offsetY = spriteRect.min.y * invTextureSizeY;

                uvs[i] = new float4(sizeX, sizeY, offsetX, offsetY);
            }
            
            return uvs;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DoTs.Resources;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DoTs.Graphics
{
    [Serializable]
    public struct UVData
    {
        public float offsetX, offsetY;
        public float sizeX, sizeY;
        public int stepX, stepY;
        public int frames;
        public float frameTime;
    }
    
    [CreateAssetMenu(menuName = "DoTs/Animations/Animation data provider")]
    public class AnimationDataProvider : ResourceProviderAsset<AnimationDataProvider>
    {
        [SerializeField]
        private Mesh _mesh;
        [SerializeField]
        private Material _material;
        [SerializeField]
        private UVData _uvData;
        [SerializeField]
        private AnimationSequence[] _sequences;

        private Dictionary<AnimationEntityType, AnimationSequence> _sequencesMap;

        public AnimationSequence GetAnimationSequence(AnimationEntityType type)
        {
            return _sequencesMap.ContainsKey(type) ? _sequencesMap[type] : null;
        }

        public NativeMultiHashMap<AnimationTypeWrapper, float4> GetSequenceDataMap()
        {
            var result = new NativeMultiHashMap<AnimationTypeWrapper, float4>(32, Allocator.Persistent);
            foreach (var entry in _sequencesMap)
            {
                var sequenceData = entry.Value.GetSequenceData();
                foreach (var uv in sequenceData)
                {
                    result.Add(new AnimationTypeWrapper(entry.Key), uv);
                }
            }

            return result;
        }
        
        public Mesh Mesh => _mesh;

        public Material Material => _material;

        public UVData UvData => _uvData;

        protected override void OnEnable()
        {
            base.OnEnable();
            _sequencesMap = _sequences.ToDictionary(s => s.Type);
        }
    }
}
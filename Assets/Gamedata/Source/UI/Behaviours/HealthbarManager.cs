using System;
using DoTs.ObjectPooling;
using UnityEngine;

namespace DoTs.UI
{
    public class HealthbarManager : MonoBehaviour
    {
        [SerializeField]
        private Healthbar _healthbarPrefab;

        private UnityObjectPool<Healthbar> _healthbarPool;

        private void Update()
        {
            
        }

        private void Start()
        {
            _healthbarPool = new UnityObjectPool<Healthbar>(_healthbarPrefab, 20);
        }
    }
}
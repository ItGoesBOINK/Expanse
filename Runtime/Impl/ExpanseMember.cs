using System;
using ItGoesBoink.Dev.Expanse.Api;
using SupaFabulus.Dev.Expanse.Common;
using UnityEngine;

namespace SupaFabulus.Dev.Expanse.Impl
{
    [Serializable]
    public class ExpanseMember : MonoBehaviour, ISpatialSectorObserver
    {
        [SerializeField]
        private Rigidbody _body;

        private Sector _currentSector;
        private Vector3 _localSectorPosition;
        private Vector3Int _prevSectorIndex;
        private Vector3Int _currentSectorIndex;

        public void SetSectorIndex(Vector3Int index)
        {
            _prevSectorIndex = _currentSectorIndex;
            _currentSectorIndex = index;
        }

        public Vector3Int PrevSectorIndex
        {
            get => _prevSectorIndex;
        }

        public Vector3Int CurrentSectorIndex
        {
            get => _currentSectorIndex;
            set => SetSectorIndex(value);
        }

        public Sector CurrentSector
        {
            get => _currentSector;
            set => MoveToSector(value);
        }

        public Vector3 LocalSectorPosition
        {
            get => _localSectorPosition;
            set => _localSectorPosition = value;
        }

        
        public bool MoveToSector(Sector targetSector)
        {
            if(targetSector == null) { Debug.LogError($""); return false; }
            _currentSector = targetSector;
            return true;
        }

        private void Awake(){}
        private void Start(){}
        private void FixedUpdate() => HandleFixedUpdate();
        //private void Update() => HandleUpdate();
        
        public void HandleFixedUpdate(){}
        public void HandleUpdate(){}
    }
}
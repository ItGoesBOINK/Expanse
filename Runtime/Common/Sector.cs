using System;
using System.Collections.Generic;
using ItGoesBoink.Dev.Expanse.Api;
using SupaFabulus.Dev.Expanse.Impl.Views;
using UnityEngine;

namespace SupaFabulus.Dev.Expanse.Common
{
    public enum SectorLoadState
    {
        NotLoaded,
        Loading,
        Loaded,
        Failed
    }

    [Serializable]
    public class Sector : MonoBehaviour
    {
        public Vector3Int LocalSectorIndex
        {
            get => _localSectorIndex;
            set => _localSectorIndex = value;
        }

        public Vector3Int WorldSectorIndex
        {
            get => _worldSectorIndex;
            set => SetWorldSectorIndex(value);
        }

        [SerializeField]
        private BasicTextSectorBillboardView _billboard;
        [SerializeField]
        private Rigidbody _body;
        [SerializeField]
        private Transform _root;
        
        private bool _isInitialized;
        private SectorLoadState _loadState;
        
        private Vector3Int _localSectorIndex;
        private Vector3Int _worldSectorIndex;
        private HashSet<ISpatialSectorMember> _members;

        public Transform Root => _root;
        public Rigidbody Body => _body;

        public void InitSector
        (
            Transform memberRoot,
            List<ISpatialSectorMember> existingMembers = null
        )
        {
            if(_isInitialized || _members.Count > 0) DeInitSector();
            if (_members == null) _members = new();
            if (existingMembers != null && existingMembers.Count > 0)
            {
                foreach (var m in existingMembers)
                { if (!_members.Contains(m)) _members.Add(m); }
            }
        }

        private void DestroyChildren()
        {
            int i, count;
            Transform c;
            count = _root.childCount;
            for (i = count - 1; i >= 0; i--)
            {
                c = _root.GetChild(i);
#if UNITY_EDITOR
                DestroyImmediate(c.gameObject);
#else
                Destroy(s.gameObject);
#endif
            }
        }

        public void SetWorldSectorIndex(Vector3Int i)
        {
            _worldSectorIndex = i;
            UpdateDisplay();
        }

        public void UpdateDisplay(bool recycle = false)
        {
            if(_billboard != null) _billboard.SetPosition(_worldSectorIndex, true);
            if(recycle) DestroyChildren();
        }

        public void DeInitSector()
        {
            RemoveAllMembers();
            _isInitialized = false;
        }

        public bool AddMembers(IEnumerable<ISpatialSectorMember> members)
        {
            if(members == null) { Debug.LogError($""); return false; }

            bool fail = false;
            foreach (var m in members)
            {
                if(m == null) { Debug.LogWarning($""); continue; }
                fail |= !AddMember(m);
            }

            return !fail;
        }
        
        public bool RemoveMembers(IEnumerable<ISpatialSectorMember> members)
        {
            if(members == null) { Debug.LogError($""); return false; }

            bool fail = false;
            foreach (var m in members)
            {
                if(m == null) { Debug.LogWarning($""); continue; }
                fail |= !RemoveMember(m);
            }

            return !fail;
        }

        public bool AddMember(ISpatialSectorMember member)
        {
            if (member == null) { Debug.LogError($""); return false; }
            if (_members == null) _members = new();
            if (_members.Contains(member)) { Debug.LogError($""); return false; }
            return _members.Add(member);
        }
        
        public bool RemoveMember(ISpatialSectorMember member)
        {
            if (member == null) { Debug.LogError($""); return false; }
            if (_members == null) { Debug.LogError($""); return false; }
            if (!_members.Contains(member)) { Debug.LogError($""); return false; }
            return _members.Remove(member);
        }
        
        private void RemoveAllMembers(){}
        private void FindChildMembers(){}
        
        public void LoadSectorContent(){}
        public void UnLoadSectorContent(){}
        
        private void HandleSectorContentLoadComplete(){}
        private void HandleSectorContentBuildComplete(){}
        private void HandleSectorContentCleanUpComplete(){}
    }
}
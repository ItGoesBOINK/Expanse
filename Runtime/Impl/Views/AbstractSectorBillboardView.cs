using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupaFabulus.Dev.Expanse.Impl.Views
{
    [Serializable]
    public abstract class AbstractSectorBillboardView<TView> : MonoBehaviour
        where TView : Component
    {
        [SerializeField] 
        protected List<TView> _fields;
        [SerializeField]
        protected Vector3Int _sectorPos;

        public void SetPosition(Vector3Int pos, bool update = false)
        {
            _sectorPos = pos;
            if(update) UpdateView();
        }

        public virtual string BillboardText => 
            $"[{_sectorPos.x}:{_sectorPos.y}:{_sectorPos.z}]";

        public abstract void UpdateView();
    }
}
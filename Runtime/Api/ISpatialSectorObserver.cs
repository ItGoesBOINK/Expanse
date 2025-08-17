using UnityEngine;

namespace ItGoesBoink.Dev.Expanse.Api
{
    public interface ISpatialSectorObserver : ISpatialSectorMember
    {
        Vector3Int PrevSectorIndex { get; }
        Vector3Int CurrentSectorIndex { get; set; }
        void SetSectorIndex(Vector3Int index);
    }
}
using SupaFabulus.Dev.Expanse.Common;
using UnityEngine;

namespace ItGoesBoink.Dev.Expanse.Api
{
    public interface ISpatialSectorMember : IGameObject
    {
        bool MoveToSector(Sector targetSector);
        Sector CurrentSector { get; set; }
        Vector3 LocalSectorPosition { get; }
    }
}
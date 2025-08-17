using System;
using System.Collections.Generic;
using ItGoesBoink.Dev.Expanse.Api;
using SupaFabulus.Dev.Expanse.Common;
using UnityEngine;

namespace SupaFabulus.Dev.Expanse.Impl
{
    [Serializable]
    public class SectorManager : MonoBehaviour
    {
        private const int SIZE = 5;
        private const int SIZESQ = SIZE * SIZE;
        private const int SECTOR_COUNT = SIZE * SIZE * SIZE;

        [SerializeField]
        private Sector _sectorPrefab;
        [SerializeField]
        private GameObject _instancePrefab;
        [SerializeField]
        [Range(1, 8192)]
        private int _sectorSize = 1024;
        
        [SerializeField]
        [Range(1, 32)]
        private int _sampleDensity = 8;
        [SerializeField]
        [Range(-10, 10)]
        private float _sampleJitter = 0f;
        [SerializeField]
        [Range(0.0001f, 1000f)]
        private float _sampleScale = 1f;
        [SerializeField]
        [Range(-1, 1)]
        private float _sampleMin = 0.9f;
        [SerializeField]
        [Range(-1, 1)]
        private float _sampleMax = 1f;
        [SerializeField]
        [Range(0.1f, 0.9f)]
        private float _sampleCoverage = 0.75f;
        [SerializeField]
        [Range(1f, 250f)]
        private float _objectScale = 10f;
        
        [SerializeField]
        private bool _wrapHeight = true;
        [SerializeField]
        private Rigidbody _observerRigidbody;
        
        private bool _isInitialized = false;
        private ISpatialSectorObserver _observer;
        private Transform _otx;
        private Sector[] _sectors;
        private HashSet<Sector> _sectorSet;
        private Dictionary<Vector3Int, Sector> _sectorMap;
        private Vector3Int _deltaIndex;
        private Vector3Int _wrap;
        private Vector3Int _polarity;
        private Vector3 _absPos;
        private Vector3 _wrappedPos;
        private bool _sectorChanged = false;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * _sectorSize * 2f);
        }


        private void Awake()
        {
            CacheObserver();
            InitSectors();
        }
        private void Start(){}
        private void FixedUpdate() => HandleFixedUpdate();
        //private void Update() => HandleUpdate();

        public void HandleFixedUpdate()
        {
            if (CheckObserverChangedSector())
            {
                WrapObserver();
                WrapSectors();
                //MoveSectors();
                //UpdateSectors();
            }
        }

        private void WrapObserver()
        {
            _observer.CurrentSectorIndex += _deltaIndex;
            _otx.position = _wrappedPos;
        }

        public void HandleUpdate(){}


        public bool Init()
        {
            if(_isInitialized) { Debug.LogError($""); return false; }
            
            bool hasObserver = CacheObserver();
            bool sectorsReady = InitSectors();
            
            if(!hasObserver) { Debug.LogError($""); }
            if(!sectorsReady) { Debug.LogError($""); }

            _sectors = new Sector[SIZESQ];
            _sectorSet = new();
            
            _isInitialized = hasObserver && 
                             sectorsReady;
            
            return _isInitialized;
        }

        public void DeInit()
        {
            DestroySectors();
            _observer = null;
            _isInitialized = false;
        }

        private int SectorIndex(int x, int y, int z) => x + (y * SIZE) + (z * SIZESQ);  
        private int SectorIndex(Vector3Int pos) =>
            pos.x + (pos.y * SIZE) + (pos.z * SIZESQ);
        public Sector GetLocalSector(int x, int y, int z)
        {
            if (_sectors == null) { Debug.LogError($""); return default; }
            if (_sectors.Length != SECTOR_COUNT) { Debug.LogError($""); return default; }
            return _sectors[x + (y * SIZE) + (z * SIZESQ)];
        }

        private bool CacheObserver()
        {
            if(_observerRigidbody == null) { Debug.LogError($""); return false; }
            _observer = _observerRigidbody.gameObject.GetComponent<ISpatialSectorObserver>();
            return _observer != null;
        }

        private bool InitSectors()
        {
            if (_sectors == null) _sectors = new Sector[SECTOR_COUNT];
            else DestroySectors();

            _sectorSet = new();
            Sector s;
            Vector3Int origin = _observer.CurrentSectorIndex;
            for (int i = 0; i < SECTOR_COUNT; i++)
            {
                Vector3Int nidx = SerialIndexToNormalizedIndex(i);
                Vector3Int cidx = nidx - DeNormalizedOffset;
                Vector3Int idx = origin + cidx;
                Debug.Log($"{nidx} -> {cidx}");
                s = Instantiate(_sectorPrefab, cidx * _sectorSize, Quaternion.Euler(Vector3.zero), transform);
                s.WorldSectorIndex = idx;
                s.LocalSectorIndex = cidx;
                PopulateSector(s);
                //s.Body.position = cidx * _sectorSize;
                _sectors[i] = s;
                _sectorSet.Add(s);
            }

            return true;
        }
        
        private Vector3Int SerialIndexToNormalizedIndex(int i) => new Vector3Int
        (
            i % SIZE,
            ((i - (i % SIZE)) / SIZE) % SIZE,
            (i - (i % SIZESQ)) / SIZESQ
        );

        private Vector3Int DeNormalizedOffset =>
            (Vector3Int.one * (Mathf.CeilToInt(SIZE / 2f) - 1));

        private Vector3Int SerialIndexRoCenteredIndex(int i) =>
            SerialIndexToNormalizedIndex(i) - DeNormalizedOffset;

        private Vector3Int WorldSpaceIndexToLocalIndex(Vector3Int wsi) =>
            wsi - _observer.CurrentSectorIndex;

        private Vector3Int WorldSpaceIndexToNormalizedIndex(Vector3Int wsi) =>
            (wsi - _observer.CurrentSectorIndex) + DeNormalizedOffset;

        private Vector3Int LocalIndexToNormalizedIndex(Vector3Int lsi) =>
            lsi + DeNormalizedOffset;
        

        private bool NormalizedIndexIsOutOfBounds(Vector3Int normIndex) => 
            (normIndex.x < 0 || normIndex.x >= SIZE) ||
            (normIndex.y < 0 || normIndex.y >= SIZE) ||
            (normIndex.z < 0 || normIndex.z >= SIZE);
        private bool NormalizedIndexIsOnEdge(Vector3Int normIndex) => 
            (normIndex.x == 0 || normIndex.x == SIZE-1) ||
            (normIndex.y == 0 || normIndex.y == SIZE-1) ||
            (normIndex.z == 0 || normIndex.z == SIZE-1);


        private void DeInitSectors(){}

        private void DestroySectors()
        {
            if (_sectors == null) return;
            Sector s;
            for (int i = 0; i < SECTOR_COUNT; i++)
            {
                s = _sectors[i];
                _sectors[i].DeInitSector();
                
                #if UNITY_EDITOR
                DestroyImmediate(s.gameObject);
                #else
                Destroy(s.gameObject);
                #endif
            }
        }
        
        

        private void WrapSectors()
        {
            int i;
            Sector s;
            Transform stx;
            Vector3 spos, moved, delta;
            Vector3Int dnormIdx = DeNormalizedOffset;
            Vector3 dnorm = dnormIdx * _sectorSize;
            Vector3 norm;
            Vector3Int wrapped, wrappedNorm;
            Vector3Int shifted, sn, normIdx;
            Vector3Int lsi, wsi;
            Vector3Int originIdx = _observer.CurrentSectorIndex;

            delta = (_deltaIndex * _sectorSize);

            for (i = 0; i < SECTOR_COUNT; i++)
            {
                s = _sectors[i];
                lsi = s.LocalSectorIndex;
                normIdx = lsi + dnormIdx;
                sn = normIdx - _deltaIndex;
                wrappedNorm = new Vector3Int
                (
                    sn.x < 0 ? SIZE+sn.x : sn.x >= SIZE ? sn.x-SIZE : sn.x,
                    sn.y < 0 ? SIZE+sn.y : sn.y >= SIZE ? sn.y-SIZE : sn.y,
                    sn.z < 0 ? SIZE+sn.z : sn.z >= SIZE ? sn.z-SIZE : sn.z
                );
                wrapped = wrappedNorm - dnormIdx;
                wsi = originIdx + wrapped;

                s.LocalSectorIndex = wrapped;
                s.WorldSectorIndex = wsi;
                
                stx = s.transform;
                stx.position = wrapped * _sectorSize;
                bool recycle = wrapped != sn;
                s.UpdateDisplay(recycle);
                if(recycle) PopulateSector(s); 
            }
        }

        private void PopulateSector(Sector s)
        {
            Vector3Int originIdx = _observer.CurrentSectorIndex;
            Vector3Int idxNorm, idx;
            Vector3Int deNorm = DeNormalizedOffset;
            Vector3 ws;
            float size = (_sectorSize * _sampleCoverage);
            float inc = size / (float)_sampleDensity;
            Vector3 local;
            float n;
            GameObject inst;
            Transform tx;
            Quaternion rot = Quaternion.Euler(Vector3.zero);
            
            for (int z = 0; z < _sampleDensity; z++)
            for (int y = 0; y < _sampleDensity; y++)
            for (int x = 0; x < _sampleDensity; x++)
            {
                idx = new(x, y, z);
                local = ((Vector3)idx * inc) - (Vector3.one * (size * 0.5f));
                ws = (local + (s.WorldSectorIndex * _sectorSize)) / (_sectorSize);
                n = Mathf.PerlinNoise1D((Mathf.PerlinNoise1D(ws.x) + Mathf.PerlinNoise1D(ws.y) + Mathf.PerlinNoise1D(ws.z)) / 3f);
                Debug.Log($"Noise @ {ws}: {n}");
                
                if (n >= _sampleMin && n <= _sampleMax)
                {
                    inst = Instantiate(_instancePrefab, s.Root);
                    tx = inst.transform;
                    tx.localPosition = local;
                    tx.localScale = (Vector3.one) + 
                                    (Vector3.one * (_objectScale * ((n - _sampleMin) / (_sampleMax - _sampleMin))));
                }
            }
        }



        private void MoveSectors()
        {
            Vector3Int idx, newIdx, normIdx;
            Vector3 newPos;
            Rigidbody rb;
            Sector s;
            Vector3Int oidx = _observer.CurrentSectorIndex;

            int i;

            for(i = 0; i < SIZESQ; i++)
            {
                s = _sectors[i];
                idx = s.WorldSectorIndex;
                newIdx = idx + _deltaIndex;
                normIdx = WorldSpaceIndexToNormalizedIndex(newIdx);
                if (NormalizedIndexIsOutOfBounds(normIdx))
                {
                    _sectors[i] = null;
#if UNITY_EDITOR
                    DestroyImmediate(s.gameObject);
#else
                Destroy(s.gameObject);
#endif
                }
            }
            
            for(i = 0; i < SIZESQ; i++)
            {
                s = _sectors[i];
                if (s == null) continue;
                idx = s.WorldSectorIndex;
                newIdx = idx + _deltaIndex;
                normIdx = WorldSpaceIndexToNormalizedIndex(newIdx);
                if (NormalizedIndexIsOutOfBounds(normIdx)) continue;
                
                s.WorldSectorIndex = newIdx;
                newPos = (newIdx - oidx) * _sectorSize;
                rb = s.Body;
                rb.position = newPos;
            }
            
            for(i = 0; i < SIZESQ; i++)
            {
                if (_sectors[i] != null) continue;
                Vector3Int o = SerialIndexToNormalizedIndex(i) - DeNormalizedOffset;
                
                newIdx = oidx + o;
                newPos = o * _sectorSize;

                s = Instantiate(_sectorPrefab, newPos, Quaternion.Euler(Vector3.zero), transform);
                //rb = s.Body;
                //rb.position = newPos;
                s.WorldSectorIndex = newIdx;
                _sectors[i] = s;
            }
        }

        private bool CheckObserverChangedSector()
        {
            _sectorChanged = false;
            _deltaIndex = Vector3Int.zero;
            
            if(_observer == null) { Debug.LogError($""); return false; }
            if (_otx == null) _otx = _observer.transform;
            
            Vector3 pos = _otx.position;
            
            _absPos = new Vector3
            (
                Mathf.Abs(pos.x),
                Mathf.Abs(pos.y),
                Mathf.Abs(pos.z)
            ); 
            _wrap = new Vector3Int
            (
                _absPos.x > _sectorSize ? -1 : 1,
                _absPos.y > _sectorSize ? -1 : 1,
                _absPos.z > _sectorSize ? -1 : 1
            );
            _wrappedPos = new Vector3
            (
                pos.x * _wrap.x,
                pos.y * _wrap.y,
                pos.z * _wrap.z
            );
            _wrappedPos = new Vector3
            (
                _wrap.x < 1 ? 0 : pos.x,
                _wrap.y < 1 ? 0 : pos.y,
                _wrap.z < 1 ? 0 : pos.z
            );
            _polarity = new Vector3Int
            (
                Mathf.RoundToInt(_absPos.x / pos.x),
                Mathf.RoundToInt(_absPos.y / pos.y),
                Mathf.RoundToInt(_absPos.z / pos.z)
            );
            _deltaIndex = new Vector3Int
            (
                Math.Clamp(-_wrap.x, 0, 1) * _polarity.x,
                Math.Clamp(-_wrap.y, 0, 1) * _polarity.y,
                Math.Clamp(-_wrap.z, 0, 1) * _polarity.z
            );

            _sectorChanged = _deltaIndex != Vector3Int.zero;
            
            return _sectorChanged;
        }
        
        private void UpdateSectors()
        {
            Sector s, r;
            int idx, idxR;
            Vector3Int i, li, oFwd, oRev, d;
            bool oobFwd, oobRev;

            d = _deltaIndex;
            for (int z = 0; z < SIZE; z++)
            for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                i = new(x, y, z);
                li = i - DeNormalizedOffset;
                oRev = i - d;
                oFwd = i + d;
                
                oobRev = NormalizedIndexIsOutOfBounds(oRev);
                if (oobRev) continue;
                
                oobFwd = NormalizedIndexIsOutOfBounds(oFwd);
                
                idxR = oRev.x + (oRev.y * SIZE) + (oRev.z * SIZESQ);
                idx = x + (y * SIZE) + (z * SIZESQ);
                
                r = _sectors[idxR];
                s = _sectors[idx];

                if (NormalizedIndexIsOnEdge(i))
                {
                    // Edge: Preloaders, Imposters, No Sim (SIZE^3 - (SIZE-2)^3)
                    
                    
                }
                else
                {
                    // Buffer: Snapping Members, Partial Sim (SIZE^3 - 1)
                    if (i == Vector3Int.zero)
                    {
                        // Center: Wrapped Observer Bounds, Full Sim (1)
                    }


                }
            }
                  
            
        }
    }
}
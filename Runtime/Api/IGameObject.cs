using UnityEngine;

namespace ItGoesBoink.Dev.Expanse.Api
{
    public interface IGameObject
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }
}
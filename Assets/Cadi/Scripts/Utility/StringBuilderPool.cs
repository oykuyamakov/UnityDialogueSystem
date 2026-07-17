using UnityEngine.Pool;
using System.Text;

namespace Cadi.Scripts.Utility
{
    public static class StringBuilderPool
    {
        private const int c_KDefaultCapacity = 8;
        private const int c_KMaxSize = 32;
        private const int c_KMaxRetainedCapacity = 1024;

        private static readonly ObjectPool<StringBuilder> s_Pool = new ObjectPool<StringBuilder>(
            createFunc: () => new StringBuilder(),
            actionOnGet: sb => sb.Clear(),
            actionOnRelease: sb =>
            {
                if (sb.Capacity > c_KMaxRetainedCapacity)
                {
                    sb.Capacity = c_KMaxRetainedCapacity;
                }
            },
            actionOnDestroy: null,
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            collectionCheck: true,
#else
            collectionCheck: false,
#endif
            defaultCapacity: c_KDefaultCapacity,
            maxSize: c_KMaxSize
        );

        public static PooledObject<StringBuilder> Get(out StringBuilder builder)
            => s_Pool.Get(out builder);
    }
}
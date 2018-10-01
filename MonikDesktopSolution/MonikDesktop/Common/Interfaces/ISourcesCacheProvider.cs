using System;

namespace MonikDesktop.Common.Interfaces
{
    public interface ISourcesCacheProvider
    {
        ISourcesCache CurrentCache { get; set; }
        void SetCurrentCacheSource(Uri url);
    }
}

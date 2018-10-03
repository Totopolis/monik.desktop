using ReactiveUI;
using System.Collections.Generic;

namespace MonikDesktop.Common
{
    public static class ReactiveExtensions
    {
        public static void Initialize<T>(this ReactiveList<T> r, IEnumerable<T> l)
        {
            using (r.SuppressChangeNotifications())
            {
                r.Clear();
                r.AddRange(l);
            }
        }
    }
}

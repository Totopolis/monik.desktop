
using System;

namespace MonikDesktop.Common.ModelsApp
{
    public class KeepAliveItem
    {
        public Instance Instance    { get; set; }
        public DateTime Created     { get; set; }
        public string   CreatedStr  { get; set; }
        public bool     StatusIsOk  { get; set; }
        public string   Status      { get; set; }
    }
}
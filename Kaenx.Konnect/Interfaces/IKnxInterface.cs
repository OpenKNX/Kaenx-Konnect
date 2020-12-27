using System;
using System.Collections.Generic;
using System.Text;

namespace Kaenx.Konnect.Interfaces
{
    public interface IKnxInterface
    {
        string Hash { get; }
        string Name { get; set; }
        string Description { get; }
        DateTime LastFound { get; set; }
        bool IsRemote { get; set; }
    }
}

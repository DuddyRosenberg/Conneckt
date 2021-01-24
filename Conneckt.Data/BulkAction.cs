using System;
using System.Collections.Generic;
using System.Text;

namespace Conneckt.Data
{
    public enum BulkAction
    {
        AddDevice,
        DeleteDevice,
        Activate,
        ExternalPort,
        InternalPort,
        ChangeSIM,
        DeactivateAndRetaineDays,
        DeactivatePastDue
    }
}

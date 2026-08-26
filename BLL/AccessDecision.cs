using System.Collections.Generic;
using System.Linq;
using BE;

namespace BLL
{
    public static class AccessDecision
    {
        public static bool Evaluate(bool isBuiltIn, ICollection<AccessGrant> grants,
                                    Section section, Operation operation)
        {
            if (isBuiltIn)
                return true;
            if (grants == null)
                return false;
            return grants.Any(g => g.Section == section && g.Operation == operation);
        }
    }
}

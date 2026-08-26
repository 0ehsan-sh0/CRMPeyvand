using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public static class AccessGuard
    {
        public static bool Can(User user, Section section, Operation operation)
        {
            if (user == null)
                return false;

            using (var db = new DB())
            {
                var loaded = db.Users.Include("UserGroup.AccessGrants")
                                     .FirstOrDefault(i => i.id == user.id);
                if (loaded == null || loaded.UserGroup == null)
                    return false;

                return AccessDecision.Evaluate(
                    loaded.UserGroup.IsBuiltIn,
                    loaded.UserGroup.AccessGrants,
                    section,
                    operation);
            }
        }
    }
}

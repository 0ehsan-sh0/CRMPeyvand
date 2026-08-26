using System.Collections.Generic;
using System.Linq;
using BE;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class AccessDecisionTests
    {
        private static List<AccessGrant> Grants(params Section[] sections)
        {
            return sections.Select(s => new AccessGrant { Section = s, Operation = Operation.View })
                           .ToList();
        }

        [Fact]
        public void Built_in_group_passes_even_with_no_grant_rows()
        {
            Assert.True(AccessDecision.Evaluate(true, new List<AccessGrant>(),
                                                Section.Users, Operation.Delete));
        }

        [Fact]
        public void Matching_grant_passes()
        {
            Assert.True(AccessDecision.Evaluate(false, Grants(Section.Customers),
                                                Section.Customers, Operation.View));
        }

        [Fact]
        public void Missing_section_or_operation_fails()
        {
            var grants = new List<AccessGrant>
            {
                new AccessGrant { Section = Section.Customers, Operation = Operation.View },
                new AccessGrant { Section = Section.Invoices,  Operation = Operation.Delete },
            };

            Assert.False(AccessDecision.Evaluate(false, grants, Section.CatalogItems, Operation.View));
            Assert.False(AccessDecision.Evaluate(false, grants, Section.Invoices,     Operation.Edit));
        }

        [Fact]
        public void Null_or_empty_grants_fail()
        {
            Assert.False(AccessDecision.Evaluate(false, null, Section.Reports, Operation.View));
            Assert.False(AccessDecision.Evaluate(false, new List<AccessGrant>(), Section.Reports, Operation.View));
        }
    }
}

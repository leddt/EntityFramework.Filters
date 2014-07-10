namespace EntityFramework.Filters.Example
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Xunit;

    public class Examples
    {
        public Examples()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<ExampleContext>());
            SeedDb();
            Database.SetInitializer(new CreateDatabaseIfNotExists<ExampleContext>());
        }

        [Fact]
        public void Should_filter_based_on_global_value()
        {
            using (var context = new ExampleContext())
            {
                var tenant = context.Tenants.Find(1);
                context.CurrentTenant = tenant;
                context.EnableFilter("Tenant")
                    .SetParameter("tenantId", tenant.TenantId);

                Assert.Equal(1, context.BlogEntries.Count());
            }
        }

        [Fact(Skip = "Expression compilation not working quite yet")]
        public void Should_filter_based_on_specific_value()
        {
            using (var context = new ExampleContext())
            {
                context.EnableFilter("BadCategory");

                var blogEntries = context.BlogEntries
                    .ToList();

                Assert.Equal(1, blogEntries.Count);
            }
        }

        [Fact]
        public void Should_support_disabling_filter()
        {
            using (var context = new ExampleContext())
            {
                var tenant = context.Tenants.Find(1);
                context.CurrentTenant = tenant;
                context.EnableFilter("Tenant")
                    .SetParameter("tenantId", tenant.TenantId);

                // Force query to execute a first time
                context.BlogEntries.Count();

                context.DisableFilter("Tenant");
                Assert.Equal(2, context.BlogEntries.Count());
            }
        }

        [Fact]
        public void Should_support_changing_filter_parameter()
        {
            using (var context = new ExampleContext())
            {
                var tenant = context.Tenants.Find(1);
                context.CurrentTenant = tenant;
                context.EnableFilter("Tenant")
                    .SetParameter("tenantId", tenant.TenantId);

                var entry = context.BlogEntries.Find(1);
                Assert.NotNull(entry);

                tenant = context.Tenants.Find(2);
                context.CurrentTenant = tenant;
                context.EnableFilter("Tenant")
                    .SetParameter("tenantId", tenant.TenantId);

                entry = context.BlogEntries.Find(1);
                Assert.Null(entry);
            }
        }

        [Fact]
        public void Should_not_persist_filter_accross_contexts()
        {
            using (var context = new ExampleContext())
            {
                var tenant = context.Tenants.Find(1);
                context.CurrentTenant = tenant;
                context.EnableFilter("Tenant")
                    .SetParameter("tenantId", tenant.TenantId);

                // Force query to execute a first time
                context.BlogEntries.Count();
            }

            using (var context = new ExampleContext())
            {
                Assert.Equal(2, context.BlogEntries.Count());
            }
        }

        private static void SeedDb()
        {
            var configuration = new MigrationsConfiguration();
            var migrator = new DbMigrator(configuration);
            migrator.Update();
        }
    }
}
using ApiIdentitySample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiIdentitySample.FactoryEF
{
    public class DesignTimeEFFactory : IDesignTimeDbContextFactory<IdentitySampleDbContext>
    {
        public IdentitySampleDbContext CreateDbContext(string[] args)
        {
            string connecstring = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DevsecopsDB;Integrated Security=True;Connect Timeout=60;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
            DbContextOptionsBuilder optionBuilder = new DbContextOptionsBuilder<IdentitySampleDbContext>();
            optionBuilder.UseSqlServer(connecstring);

            return new IdentitySampleDbContext(optionBuilder.Options);
        }
    }
}

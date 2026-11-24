using MultiSchema.BLL.Common.Implementation;
using MultiSchema.BLL.Common.Signature;
using MultiSchema.Common.Constant;
using MultiSchema.DAL.Base;
using MultiSchema.DAL.Data;

namespace MultiSchema.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---- Database: pick the engine once; every schema lives in that one database ----
            var dbSection = builder.Configuration.GetSection("Database");
            CommonConts.DBInstance = DbProvider.Parse(dbSection["Provider"]);
            if (Enum.TryParse<enMySqlSchemaMode>(dbSection["MySqlSchemaMode"], true, out var mySqlMode))
                CommonConts.MySqlSchemaMode = mySqlMode;
            if (int.TryParse(dbSection["CommandTimeoutSeconds"], out var timeout))
                CommonConts.CommandTimeoutSeconds = timeout;

            DBConnection.Configuration = builder.Configuration;

            // One context for mrm/sec/aud/lic/app/dat/rpt. Options come from OnConfiguring (ECL3 style),
            // so `new MultiSchemaContext()` in legacy-style code and DI-injected instances behave the same.
            builder.Services.AddDbContext<MultiSchemaContext>();

            builder.Services.AddScoped<IModelInventoryService, ModelInventoryService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

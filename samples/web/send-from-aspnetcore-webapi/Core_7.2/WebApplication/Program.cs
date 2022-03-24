using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Sql;

public class Order
{
    public virtual string OrderId { get; set; }
    public virtual decimal Value { get; set; }
}

public class ReceiverDataContext :
    DbContext
{
    public ReceiverDataContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var orders = modelBuilder.Entity<Order>();
        orders.ToTable("Orders");
        orders.HasKey(x => x.OrderId);
        orders.Property(x => x.Value);
    }
}



class WebCurrentSessionHolder
{
    public IUnitOfWorkSession Current => pipelineContext.Value?.Session;

    public void SetCurrentSession(IUnitOfWorkSession session)
    {
        pipelineContext.Value.Session = session;
    }

    public IDisposable CreateScope()
    {
        if (pipelineContext.Value != null)
        {
            throw new InvalidOperationException("Attempt to overwrite an existing session context.");
        }
        var wrapper = new Wrapper();
        pipelineContext.Value = wrapper;
        return new Scope(this);
    }

    readonly AsyncLocal<Wrapper> pipelineContext = new AsyncLocal<Wrapper>();

    class Wrapper
    {
        public IUnitOfWorkSession Session;
    }

    class Scope : IDisposable
    {
        public Scope(WebCurrentSessionHolder sessionHolder)
        {
            this.sessionHolder = sessionHolder;
        }

        public void Dispose()
        {
            sessionHolder.pipelineContext.Value = null;
        }

        readonly WebCurrentSessionHolder sessionHolder;
    }
}

public interface IUnitOfWorkSession //: IMessageSession
{
    SynchronizedStorageSession StorageSession { get; }
    Task Commit();
}

class UnitOfWorkSession : IUnitOfWorkSession
{
    public SynchronizedStorageSession StorageSession { get; }
    public Task Commit()
    {
        return Task.CompletedTask;
    }
}

public class WebCurrentSessionHolderMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var holder = context.RequestServices.GetService<WebCurrentSessionHolder>();

        using (var scope = holder.CreateScope())
        {
            var session = new UnitOfWorkSession();
            holder.SetCurrentSession(session);

            await next(context);

            await session.Commit();
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        var connection = @"Server=localhost,1433;Initial Catalog=sc-sql-spike;Persist Security Info=False;User ID=sa;Password=larry666!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

        using (var receiverDataContext = new ReceiverDataContext(new DbContextOptionsBuilder<ReceiverDataContext>()
                   .UseSqlServer(new SqlConnection(connection))
                   .Options))
        {
            await receiverDataContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }

        #region EndpointConfiguration
        var host = Host.CreateDefaultBuilder()
            .UseNServiceBus(context =>
            {
                var endpointConfiguration = new EndpointConfiguration("Samples.ASPNETCore.Sender");
                var transport = endpointConfiguration.UseTransport<LearningTransport>();
                transport.Routing().RouteToEndpoint(
                    assembly: typeof(MyMessage).Assembly,
                    destination: "Samples.ASPNETCore.Endpoint");

                var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
                persistence.ConnectionBuilder(() => new SqlConnection(connection));
                var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();

                endpointConfiguration.RegisterComponents(c =>
                {
                    c.RegisterSingleton(new WebCurrentSessionHolder());

                    c.ConfigureComponent(b =>
                    {
                        ISqlStorageSession session;
                        var webSession = b.Build<WebCurrentSessionHolder>().Current;
                        if (webSession != null)
                        {
                            session = webSession.StorageSession.SqlPersistenceSession();
                        }
                        else
                        {
                            session = b.Build<ISqlStorageSession>();
                        }

                        var dataContext = new ReceiverDataContext(new DbContextOptionsBuilder<ReceiverDataContext>()
                            .UseSqlServer(session.Connection)
                            .Options);

                        //Use the same underlying ADO.NET transaction
                        dataContext.Database.UseTransaction(session.Transaction);

                        //Ensure context is flushed before the transaction is committed
                        session.OnSaveChanges(s => dataContext.SaveChangesAsync());

                        return dataContext;
                    }, DependencyLifecycle.InstancePerUnitOfWork);
                });

                return endpointConfiguration;
            })
            .ConfigureWebHostDefaults(c => c.UseStartup<Startup>())
            .Build();
        #endregion

        await host.RunAsync();
    }
}

using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OrderService.GraphQL;
using OrderService.Kafka;
using SolakaDatabase.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Inject Database
var conString = builder.Configuration.GetConnectionString("MyDatabase");
builder.Services.AddDbContext<SolakaDbContext>(options =>
     options.UseSqlServer(conString)
);

// graphql
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddAuthorization();

builder.Services.AddControllers();
// DI Dependency Injection
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
            ValidateIssuer = true,
            ValidAudience = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
            ValidateIssuerSigningKey = true
        };

    });

//inject Kafka
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL();
app.MapGet("/", () => "Hello World!");

app.Run();

//===========================KAFKA======================================
var config = new ConsumerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    GroupId = "tester",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

var topic = "SOLAKA";
CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using (var consumer = new ConsumerBuilder<string, string>(config).Build())
{
    Console.WriteLine("Connected");
    consumer.Subscribe(topic);
    try
    {
        while (true)
        {
            var cr = consumer.Consume(cts.Token);
            Console.WriteLine($"Consumed record with key: {cr.Message.Key} and value: {cr.Message.Value}");

            using (var context = new SolakaDbContext())
            {
                var val2 = JsonConvert.DeserializeObject<SendDataOrder>(cr.Message.Value);

                var order = context.Orders.Where(o => o.Id == val2.TransactionId).FirstOrDefault();
                //var order = context.Orders.Include(o=>o.OrderDetails).Where(o=>o.Id==val2.TransactionId).FirstOrDefault();
               
                order.Status = val2.PaymentStatus;

                //if (val2.PaymentStatus == "Completed")
                //{
                //    foreach (var item in order.OrderDetails)
                //    {
                //        var product = context.Products.Where(p => p.Id == item.ProductId).FirstOrDefault();
                //        //if (product.Stock <= item.Quantity) continue; 
                //        if (product.Stock < item.Quantity)
                //        {
                //            order.Status = "Gagal";
                //            break;
                //        }

                //        product.Stock -= item.Quantity;
                //    }
                //}

                context.Orders.Update(order);
                await context.SaveChangesAsync();

            }
        }
    }
    catch (OperationCanceledException)
    {
        // Ctrl-C was pressed.
    }
    finally
    {
        consumer.Close();
    }
}
//=================================================================
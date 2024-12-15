using Quartz;
using WebApi.Jobs;
using WebApi.Models;
using WebApi.Services;
namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<LoaderDatabaseSettings>(builder.Configuration.GetSection(nameof(LoaderDatabaseSettings)));
        builder.Services.AddSingleton<LoaderTaskService>();
        builder.Services.Configure<ArticleDatabaseSettings>(builder.Configuration.GetSection(nameof(ArticleDatabaseSettings)));
        builder.Services.AddSingleton<ArticleService>();

        builder.Services.AddTransient<SiteCrawler>();
        builder.Services.AddTransient<TaskHandler>();

        var taskHandlerInterval = builder.Configuration.GetValue<int>("TaskSettings:Interval");
        var siteCrawlerInterval = builder.Configuration.GetValue<int>("CrawlerSettings:Interval");
        var initializeSiteCrawler = builder.Configuration.GetValue<bool>("CrawlerSettings:Initialize");
        // Add services to the container.
        builder.Services.AddQuartz(q =>
        {
            var taskHandlerJobKey = new JobKey(nameof(TaskHandler));
            q.AddJob<TaskHandler>(opts => opts.WithIdentity(taskHandlerJobKey));
            q.AddTrigger(trigger => trigger
                .ForJob(taskHandlerJobKey)
                .WithSimpleSchedule(sch =>
                    sch.WithIntervalInSeconds(taskHandlerInterval).RepeatForever()));
            if (initializeSiteCrawler) {
                var siteCrawlerJobKey = new JobKey(nameof(SiteCrawler));
                q.AddJob<SiteCrawler>(opts => opts.WithIdentity(siteCrawlerJobKey));
                q.AddTrigger(trigger => trigger
                    .ForJob(siteCrawlerJobKey)
                    .WithSimpleSchedule(sch =>
                        sch.WithIntervalInSeconds(siteCrawlerInterval).RepeatForever()));
            }
        });
        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder
                        // .WithOrigins("http://localhost:3000")
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}

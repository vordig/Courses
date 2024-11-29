using Asp.Versioning;

namespace Courses.WebAPI.ApiVersioning;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddDefaultApiVersioning(this IHostApplicationBuilder builder)
    {
        var apiVersionManager = new ApiVersionManager();
        builder.Services.AddSingleton(apiVersionManager);
        
        var withApiVersioning = builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            // options.Policies.Sunset(new ApiVersion(1.0))
            //     .Effective(DateTime.SpecifyKind(new DateTime(2024, 8, 1), DateTimeKind.Utc));
        });
        builder.AddDefaultOpenApi(withApiVersioning);
        
        return builder;
    }

    public static IApplicationBuilder UseDefaultApiVersioning(this WebApplication app)
    {
        var apiVersioningOptions = new ApiVersioningOptions();
        app.Configuration.GetSection(ApiVersioningOptions.ApiVersions).Bind(apiVersioningOptions);
        
        var apiVersionSetBuilder = app.NewApiVersionSet();

        foreach (var apiVersion in apiVersioningOptions.SupportedApiVersions)
            apiVersionSetBuilder.HasApiVersion(apiVersion);
        
        foreach (var apiVersion in apiVersioningOptions.DeprecatedApiVersions)
            apiVersionSetBuilder.HasDeprecatedApiVersion(apiVersion);
        
        var apiVersionSet = apiVersionSetBuilder
            .ReportApiVersions()
            .Build();

        var apiVersionManager = app.Services.GetRequiredService<ApiVersionManager>();
        apiVersionManager.WithApiVersionSet(apiVersionSet);
        
        app.UseDefaultOpenApi();
        
        return app;
    }
}
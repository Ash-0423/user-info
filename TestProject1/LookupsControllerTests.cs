using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserInfoManager.Controllers.Api;
using UserInfoManager.Data;

namespace TestProject1;

public class LookupsControllerTests
{
    private readonly LookupsController _controller;

    public LookupsControllerTests()
    {
        var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient(sp => new AdoNetHelper(configuration.GetConnectionString("DefaultConnection")));
        services.AddTransient<LookupsDataAccess>();

        var serviceProvider = services.BuildServiceProvider();

        var lookupsDataAccess = serviceProvider.GetRequiredService<LookupsDataAccess>();

        _controller = new LookupsController(lookupsDataAccess);
    }

//   [Fact]
 //   public void GetAddress_ValidInput_ReturnsJsonResult()
 //   {
 //       var keyword = "a"; 
 //       var lookupType = "Country";

        // Act
//        var result = _controller.GetAddress(keyword, lookupType) as JsonResult;

        // Assert
//        Assert.NotNull(result);
//        var data = result.Value as dynamic;
//        Assert.NotNull(data);
//        var c = result.Value.GetType().GetProperty("done").GetValue(result.Value);
//       Assert.Equal(1, c);
//    }
}
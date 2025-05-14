using Microsoft.Data.SqlClient;
using System.Data;
using UserInfoManager.Data;

namespace UserInfoManager.Service;

public class DeleteUnverifiedUsersService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly AdoNetHelper _dbHelper;

    public DeleteUnverifiedUsersService(AdoNetHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        var sevenDaysAgo = DateTime.Now.AddDays(-7);

        try
        {
            // 调用存储过程
            SqlParameter parameter = new SqlParameter("@SevenDaysAgo", sevenDaysAgo);
            _dbHelper.ExecuteNonQuery("[dbo].[sp_DeleteUnverifiedUsers]", CommandType.StoredProcedure, null, null, parameter);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting unverified users: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
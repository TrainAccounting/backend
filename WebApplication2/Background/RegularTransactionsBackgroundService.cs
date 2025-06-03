using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trainacc.Services;

namespace Trainacc.Background
{
    public class RegularTransactionsBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RegularTransactionsBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<RegularTransactionsService>();
                        await service.ApplyRegularTransactionsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при применении регулярных транзакций: {ex.Message}");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

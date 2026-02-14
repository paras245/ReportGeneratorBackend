using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReportGeneratorBackend.Data;
using ReportGeneratorBackend.Models;
using System;

namespace ReportGeneratorBackend.BackgroundService
{
    // Background service to process reports asynchronously
    public class ReportProcessorService : Microsoft.Extensions.Hosting.BackgroundService
    {
        #region Dependencies

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<ReportHub> _hub;
        private readonly ILogger<ReportProcessorService> _logger;

        #endregion

        #region Constructor

        public ReportProcessorService(IServiceScopeFactory scopeFactory,
                                       IHubContext<ReportHub> hub,
                                       ILogger<ReportProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
            _logger = logger;
        }

        #endregion

        #region Execution Logic

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report Processor Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a scope to resolve scoped services
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Find oldest pending job
                    var job = await db.ReportJobs
                        .Where(x => x.Status == ReportStatus.Pending)
                        .OrderBy(x => x.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (job != null)
                    {
                        _logger.LogInformation($"Processing job {job.Id}");
                        
                        // 1. Mark as Processing & Notify
                        job.Status = ReportStatus.Processing;
                        await db.SaveChangesAsync(stoppingToken);
                        await _hub.Clients.All.SendAsync("ReceiveUpdate", job, stoppingToken);

                        // 2. Simulate Work
                        await Task.Delay(5000, stoppingToken);

                        // 3. Mark as Completed & Notify
                        job.Status = ReportStatus.Completed;
                        await db.SaveChangesAsync(stoppingToken);
                        await _hub.Clients.All.SendAsync("ReceiveUpdate", job, stoppingToken);
                        
                        _logger.LogInformation($"Job {job.Id} completed.");
                    }
                    else
                    {
                        // No work? Wait before retrying
                        await Task.Delay(5000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing report job");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        #endregion
    }
}

using Microsoft.AspNetCore.SignalR;

namespace ReportGeneratorBackend
{
    #region SignalR Hub

    // Acts as a bridge between server and clients for real-time updates.
    public class ReportHub : Hub
    {
        // No client-to-server methods needed for this implementation.
    }

    #endregion
}

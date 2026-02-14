namespace ReportGeneratorBackend.Models
{
    // Domain model representing a report job in the database
    public class ReportJob
    {
        #region Properties

        public Guid Id { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        #endregion
    }

    #region Enums

    public enum ReportStatus
    {
        Pending,
        Processing,
        Completed
    }

    #endregion
}

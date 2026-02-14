namespace ReportGeneratorBackend.DTOs
{
    // Data Transfer Object for creating a new report
    public class CreateReportDto
    {
        #region Properties

        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        #endregion
    }
}

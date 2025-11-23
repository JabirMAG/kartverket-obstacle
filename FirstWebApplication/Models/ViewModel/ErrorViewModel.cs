namespace FirstWebApplication.Models
{
    /// <summary>
    /// ViewModel used to display error messages in the application. Typically shown when something goes wrong in HomeController -> Error()
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// ID that identifies the current request. Can be used for debugging and tracking in logs
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Returns true if RequestId exists (is not null or empty)
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

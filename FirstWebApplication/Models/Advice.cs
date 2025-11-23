namespace FirstWebApplication.Models
{
    /// <summary>
    /// Represents feedback from a user. Used when someone fills out and submits a feedback form
    /// </summary>
    public class Advice
    {
        /// <summary>
        /// Unique ID for the feedback (primary key in database)
        /// </summary>
        public int adviceID { get; set; }

        /// <summary>
        /// The message/content of the feedback
        /// </summary>
        public string adviceMessage { get; set; }

        /// <summary>
        /// Email address of the person submitting the feedback
        /// </summary>
        public string Email { get; set; }
    }
}
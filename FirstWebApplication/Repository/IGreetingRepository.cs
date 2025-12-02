namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository interface for generating time-based greetings
    /// </summary>
    public interface IGreetingRepository
    {
        /// <summary>
        /// Gets a greeting based on the current time of day
        /// </summary>
        /// <returns>Norwegian greeting message (God morgen!, God ettermiddag!, or God kveld!)</returns>
        string GetTimeBasedGreeting();
        
        /// <summary>
        /// Gets a greeting based on a specific hour of the day
        /// </summary>
        /// <param name="hour">The hour of the day (0-23)</param>
        /// <returns>Norwegian greeting message based on the hour</returns>
        string GetGreetingForHour(int hour);
    }
}


namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for generating time-based greetings
    /// </summary>
    public class GreetingRepository : IGreetingRepository
    {
        /// <summary>
        /// Gets a greeting based on the current time of day
        /// </summary>
        /// <returns>Norwegian greeting message (God morgen!, God ettermiddag!, or God kveld!)</returns>
        public string GetTimeBasedGreeting()
        {
            var hour = DateTime.Now.Hour;
            return GetGreetingForHour(hour);
        }

        /// <summary>
        /// Gets a greeting based on a specific hour of the day
        /// </summary>
        /// <param name="hour">The hour of the day (0-23)</param>
        /// <returns>Norwegian greeting message based on the hour</returns>
        public string GetGreetingForHour(int hour)
        {
            if (hour < 12)
                return "God morgen!";
            else if (hour < 18)
                return "God ettermiddag!";
            else
                return "God kveld!";
        }
    }
}


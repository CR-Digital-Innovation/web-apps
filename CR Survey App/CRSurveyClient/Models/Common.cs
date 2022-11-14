namespace CRSurveyClient.Models
{
    /// <summary>
    /// Enumeration used to define exicution status 
    /// </summary>
    public enum ExecutionStatus
    {
        Fail = 0,
        Success = 1
    }

    /// <summary>
    /// Class used to define exicution status of a operation and message of the operation
    /// </summary>
    public class ExecutionResponse
    {
        public ExecutionStatus ExecutionStatus { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Event type 
    /// </summary>
    public enum EventType
    {
        Submit = 0,
        Next = 1,
        Back= 2
    }
}

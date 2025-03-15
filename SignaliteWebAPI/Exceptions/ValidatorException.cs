namespace SignaliteWebAPI.Exceptions;

public class ValidatorException : Exception
{
    public ValidatorException(string message) : base(message) { }
    
    public int StatusCode { get; set; } = 400; // TODO: Find better code for validation
    public List<string> Errors { get; set; } = [];
}
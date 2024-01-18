namespace Wave.Data;

public class SmtpConfiguration {
    public required string Host { get; init; } 
    public required int Port { get; init; } 
    public required string Username { get; init; } 
    public required string Password { get; init; } 
    public required string SenderEmail { get; init; }
    public required string SenderName { get; init; }
    public bool Ssl { get; init; } = true;  
}
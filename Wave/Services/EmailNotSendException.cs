namespace Wave.Services;

public class EmailNotSendException(string message, Exception exception) : ApplicationException(message, exception);
[Serializable]
public abstract class BaseException : Exception
{
    public BaseException() { }
    public BaseException(string message, string explanation) : base(message)
    {
        Explanation = explanation;
    }
    public BaseException(string message, Exception inner) : base(message, inner) { }
    protected BaseException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    public string Explanation { get; }
    public string Documentation { get; init; }
}

[Serializable]
public class BusinessException : BaseException
{
    public BusinessException() { }
    public BusinessException(string message, string explanation) : base(message, explanation) { }
    public BusinessException(string message, Exception inner) : base(message, inner) { }
    protected BusinessException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[Serializable]
public class TechnicalException : BaseException
{
    public string ErrorMessage { get; }

    public TechnicalException() { }
    public TechnicalException(string message, string explanation) : base(message, explanation) { }
    public TechnicalException(string message, string explanation, string errorMessage) : base(message, explanation)
    {
        ErrorMessage = errorMessage;
    }

    public TechnicalException(string message, Exception inner) : base(message, inner) { }
    protected TechnicalException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

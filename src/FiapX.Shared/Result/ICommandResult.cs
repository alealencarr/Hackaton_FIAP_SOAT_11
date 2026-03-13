namespace FiapX.Shared.Result;

public interface ICommandResult
{
    List<string> Messages { get; set; }
    bool Succeeded { get; set; }
    bool Conflict { get; set; }
}

public interface ICommandResult<T> : ICommandResult
{
    T Data { get; set; }
}

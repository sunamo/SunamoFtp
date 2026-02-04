// variables names: ok
namespace SunamoFtp._public.SunamoInterfaces.Interfaces;

/// <summary>
/// Interface for objects that can report their working state
/// </summary>
public interface IWorking
{
    /// <summary>
    /// Gets a value indicating whether the operation is currently working/active
    /// </summary>
    bool IsWorking { get; }
}

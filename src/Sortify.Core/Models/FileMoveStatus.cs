namespace Sortify.Core.Models;

public enum FileMoveStatus
{
    Succeeded,
    NotConfirmed,
    SourceNotFound,
    SourceChanged,
    TargetAlreadyExists,
    InvalidRequest,
    Failed
}
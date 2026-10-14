using System;

namespace Sortify.Core.Models;

public sealed record RuleValidationError(string FieldName, string Message);
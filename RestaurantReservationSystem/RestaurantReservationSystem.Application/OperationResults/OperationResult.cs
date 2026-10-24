using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.OperationResults
{
    public class OperationResult
    {
        // PROPERTIES:
        public bool IsSuccess { get; }
        public IReadOnlyList<ValidationError> ValidationErrors { get; }
        public IReadOnlyList<ValidationError> Warnings { get; }
        public OperationErrorCode? ErrorCode { get; }
        public string? Message { get; }
        // CONSTRUCTORS:
        // Success
        protected OperationResult(
        IReadOnlyList<ValidationError>? warnings)
        {
            IsSuccess = true;
            ValidationErrors = [];
            Warnings = warnings?.ToArray() ?? [];
        }
        // Validation
        protected OperationResult(
        IReadOnlyList<ValidationError> errors,
        IReadOnlyList<ValidationError>? warnings)
        {
            ArgumentNullException.ThrowIfNull(errors);
            if (errors.Count == 0)
                throw new ArgumentException("Validation requires errors.");
            IsSuccess = false;
            ValidationErrors = errors.ToArray();
            Warnings = warnings?.ToArray() ?? [];
        }
        // Failure
        protected OperationResult(
        OperationErrorCode errorCode,
        string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty.");
            }
            IsSuccess = false;
            ValidationErrors = [];
            Warnings = [];
            ErrorCode = errorCode;
            Message = message;
        }
        // FACTORY METHODS:
        public static OperationResult Success(
        IReadOnlyList<ValidationError>? warnings = null)
        => new(warnings);
        public static OperationResult Validation(
        IReadOnlyList<ValidationError> errors,
        IReadOnlyList<ValidationError>? warnings = null)
        => new(errors, warnings);
        public static OperationResult Failure(
        OperationErrorCode errorCode,
        string message)
        => new(errorCode, message);
    }
    public sealed class OperationResult<T> : OperationResult where T : class
    {
        // PROPERTIES:
        public T? Data { get; }
        // CONSTRUCTORS:
        // Success
        private OperationResult(
        T data,
        IReadOnlyList<ValidationError>? warnings)
        : base(warnings)
        {
            ArgumentNullException.ThrowIfNull(data);
            Data = data;
        }
        // Validation
        private OperationResult(
        IReadOnlyList<ValidationError> errors,
        IReadOnlyList<ValidationError>? warnings)
        : base(errors, warnings)
        {
            Data = null;
        }
        // Failure
        private OperationResult(
        OperationErrorCode errorCode,
        string message)
        : base(errorCode, message)
        {
            Data = null;
        }
        // FACTORY METHODS:
        public static OperationResult<T> Success(T data,
        IReadOnlyList<ValidationError>? warnings = null)
        => new(data, warnings);
        public new static OperationResult<T> Validation(
        IReadOnlyList<ValidationError> errors,
        IReadOnlyList<ValidationError>? warnings = null)
        => new(errors, warnings);
        public new static OperationResult<T> Failure(
        OperationErrorCode errorCode,
        string message)
        => new(errorCode, message);
    }
}

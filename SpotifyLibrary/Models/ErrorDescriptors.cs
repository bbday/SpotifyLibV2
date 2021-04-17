using System.Collections.Generic;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Validation;

namespace SpotifyLibrary.Models
{
    internal class ErrorDescriptors : List<ErrorDescriptor>, IValidationErrors
    {
        internal static ErrorDescriptors Empty = Create();

        private ErrorDescriptors() : base()
        {
        }

        internal static ErrorDescriptors Create()
        {
            return new ErrorDescriptors();
        }

        void IValidationErrors.Add(ErrorSeverity severity, string error)
        {
            Add(new ErrorDescriptor(severity, error));
        }
    }
}

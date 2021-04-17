using SpotifyLibrary.Enums;

namespace SpotifyLibrary.Validation
{
	internal interface IValidationErrors
    {
        void Add(ErrorSeverity severity, string error);
    }
}
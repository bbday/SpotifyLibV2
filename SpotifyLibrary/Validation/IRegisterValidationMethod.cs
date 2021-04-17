namespace SpotifyLibrary.Validation
{
	internal interface IRegisterValidationMethod
    {
        void RegisterValidationMethod(string propertyName, ValidateMethod validateMethod);
    }
}

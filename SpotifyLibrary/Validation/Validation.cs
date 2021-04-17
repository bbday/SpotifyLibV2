using System;
using System.Linq.Expressions;

namespace SpotifyLibrary.Validation
{
    internal delegate void ValidateMethod(IValidationErrors errors);

    internal static class ValidationExtensions
    {
        internal static void ValidateProperty<TSender, TRet>(this TSender viewModel, 
            Expression<Func<TSender, TRet>> property, ValidateMethod validateMethod) where TSender : IRegisterValidationMethod
        {
            string propertyName = ((MemberExpression)property.Body).Member.Name;

            viewModel.RegisterValidationMethod(propertyName, validateMethod);
        }
    }
}

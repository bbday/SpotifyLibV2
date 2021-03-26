using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace SpotifyLibrary.Exceptions
{
    public class CdnException : Exception
    {

        public CdnException([NotNull] string message) : base(message)
        {
        }

        public CdnException([NotNull] Exception ex) : base(ex.Message, ex)
        {

        }
    }
}

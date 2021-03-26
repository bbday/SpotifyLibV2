﻿using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace SpotifyLibrary.Exceptions
{
    public class ChunkException : Exception
    {
        public ChunkException([NotNull] Exception cause) : base(cause.Message)
        {
        }

        protected ChunkException()
        {
        }

        public static ChunkException FromStreamError(short streamError)
        {
            return new ChunkException(new Exception("Failed due to stream error, code: " + streamError));
        }
    }
}
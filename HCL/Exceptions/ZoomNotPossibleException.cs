﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.HCL.Exceptions
{
    [Serializable]
    public class ZoomNotPossibleException : Exception
    {
        public ZoomNotPossibleException(string message) : base(message)
        {
            LocalMessage = message;
        }

        public ZoomNotPossibleException(string message, Exception innerException) : base(message, innerException)
        {
            LocalMessage = message;
            LocalInnerException = innerException;
        }

        public string LocalMessage { get; set; }

        public Exception LocalInnerException { get; set; }

        public ZoomNotPossibleException()
        {
        }

        protected ZoomNotPossibleException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(
            serializationInfo, streamingContext)
        {
        }
    }
}
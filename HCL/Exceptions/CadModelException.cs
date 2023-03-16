using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.HCL.Exceptions
{
    [Serializable]
    public class CadModelException : Exception
    {
        public CadModelException(string message) : base(message)
        {
            LocalMessage = message;
        }

        public CadModelException(string message, Exception innerException) : base(message, innerException)
        {
            LocalMessage = message;
            LocalInnerException = innerException;
        }

        public string LocalMessage { get; set; }

        public Exception LocalInnerException { get; set; }

        public CadModelException()
        {
        }

        protected CadModelException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorExtractor
{
    [Serializable]
    class IrreversibleMatrixException: Exception
    {
        public IrreversibleMatrixException() : base() { }
        public IrreversibleMatrixException(string message) : base(message) { }
        public IrreversibleMatrixException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected IrreversibleMatrixException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

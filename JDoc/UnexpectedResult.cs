using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JDoc
{
    public class UnexpectedResult : Exception
    {
        private Result result;

        public UnexpectedResult(Result result)
        {
            this.result = result;
        }
    }
}

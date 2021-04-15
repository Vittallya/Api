using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Web.Logger
{
    public interface ILogStorage
    {
        public void Store(LoggerModel log);
    }
}

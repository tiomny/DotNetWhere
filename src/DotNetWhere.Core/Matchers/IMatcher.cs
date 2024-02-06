using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWhere.Core.Matchers;

internal interface IMatcher
{
    bool Match(string name);
}

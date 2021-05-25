using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mastersign.Expressions.Language
{
    internal interface IRightPart
    {
        ExpressionElement BuildExpressionElement(ExpressionElement leftPart);
    }
}

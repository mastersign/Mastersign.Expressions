using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.expressions.language
{
    internal interface IRightPart
    {
        ExpressionElement BuildExpressionElement(ExpressionElement leftPart);
    }
}

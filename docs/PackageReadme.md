# Mastersign.Expressions

> A parser and compiler for a small EXCEL like expression language inside your .NET applications.

_Mastersign.Expression_ is a small DSL (domain specific language) – a little like the MS EXCEL formular language. Its purpose is to provide a simple expression language with a set of predefined functions for math and string manipulation. Expressions are parsed via a parser, based on the [Sprache](https://github.com/sprache) parser framework, and compiled via System.Linq.Expressions into a lambda delegate, and can therefore, used repeatedly with acceptable performance.

## Features

_Mastersign.Expression_ supports numeric, boolean and string literals, operators for numeric operations, string concatenation, logical combination and comparison. Grouping is done with parantheses. Function calls have a C-style-syntax. Functions, variables, constants, and parameters are provided via an evaluation context, which can be easily extended by the developer.

It comes with a set of predefined functions for math, string manipulation and regular expressions.

The evaluation context can be configured to ignore the case of operator keywords, literal keywords, variables, parameters, and function names. The quote style for strings can be configured too.

## Expression Examples

* `1 + 2`
* `pi * (100f + 32.0/a)`
* `"\tName: " & name`
* `"result = " & (sin(pi * 2.0 + x) / 10)`

## Simple Usage Scenario

```csharp
using System;
using Mastersign.Expressions;

static class Program
{
    static void Main()
    {
        // create the evaluation context for the expression
        var context = new EvaluationContext();
        // load the default packages with functions and constants (math, string, ...)
        context.LoadAllPackages();
        // add a custom variable
        context.SetVariable("x", 4);
        // add a custom function
        context.AddFunction("neg", (Func<double, double>)(v => -v));
        // set parameter list
        context.SetParameters(new ParameterInfo("a", typeof(int)));
        // compile the expression into a lamda delegate
        var fun = context.CompileExpression<int, double>("sin(pi * neg(10 + x)) + a");
        // call the delegate and write the result to the console
        Console.WriteLine(fun(50));
    }
}
```

## License

This project is published under the MIT license.

Copyright &copy; by Tobias Kiertscher <dev@mastersign.de>

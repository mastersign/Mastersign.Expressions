# Example

To use _Mastersign.Expressions_ you need only to create an instance of `EvaluationContext`,
maybe add custom variables and functions, and off you go!

```csharp
using System;
using de.mastersign.expressions;
static class Program
{
    static void Main()
    {
        // Create a main evaluation context for all your expressions
        var mainContext = new EvaluationContext();

        // Load all default packages (math, string manipulation, ...)
        mainContext.LoadAllPackages();

        // Add a custom function to the main context
        mainContext.AddFunction("neg", (Func<double, double>)(v => -v));

        // Create an evaluation context (A), which inherits all variables 
        // and functions from the main context
        var contextA = new EvaluationContext(mainContext);

        // Add a custom variable to context A
        contextA.SetVariable("x", 4);

        // Configure the list of parameters of context A 
        contextA.SetParameters(new ParameterInfo("a", typeof(int)));

        // Compile an expression into a lambda delegate using context A
        var exprA = "sin(pi * neg(10.0 + x)) + a";
        var funA = contextA.CompileExpression<int, double>(exprA);

        // Create a second evaluation context (B)
        var contextB = new EvaluationContext(mainContext);

        // Add a custom variable to context B
        contextB.SetVariable("x", 5);

        // Compile an expression into a lambda delegate using context B
        var exprB = "\"High \" & x & \"!\"";
        var funB = contextB.CompileExpression<string>(exprB);

        // Call the delegates and write the results to the console
        Console.WriteLine("{0} -> {1}", exprA, funA(2));
        Console.WriteLine("{0} -> {1}", exprB, funB());

        // The output looks like this:
        // > sin(pi * neg(10.0 + x)) + a -> 2
        // > "High " & x & "!" -> High 5!
    }
}
```

In the version 0.4.0 of Mastersign.Expressions the support for reading members is allways activated, since version 0.4.1 additional language capabilities are activated by `ExpressionContext.Capabilities`. The following example demonstrates the activation of optional language features.

```csharp
using System;
using de.mastersign.expressions;
static class Program
{
    static void Main()
    {
        // Create a evaluation context
        var context = new EvaluationContext();

        // Activate optional capabilities
        context.Capabilities = LanguageCapabilities.MemberRead;

        // Compile an expression into a lambda delegate using context
        var expr = "(\"abc\" & \"def\").Length";
        var fun = context.CompileExpression<int>(expr);

        // Call the delegate and write the result to the console
        Console.WriteLine("{0} -> {1}", expr, fun());

        // The output looks like this:
        // > ("abc" & "def").Length -> 6
    }
}
```
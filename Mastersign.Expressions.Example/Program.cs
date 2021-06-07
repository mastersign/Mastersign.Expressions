using System;
using Mastersign.Expressions;

static class Program
{
    static void Main()
    {
        // Prepare some language options
        var langOptions = new LanguageOptionsBuilder()
            .IgnoreBooleanLiteralCase()
            .IgnoreNullLiteralCase()
            .WithConditionalName("iif")
            .Build();

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

            // derive new language options
            // and assign them to the second eval context
        contextB.Options = langOptions.Derive()
            .IgnoreVariableNameCase()
            .Build();

        // Add a custom variable to context B
        contextB.SetVariable("x", 0);

        // Compile an expression into a lambda delegate using context B
        var exprB = "\"High \" & X & \"!\"";
        var funB = contextB.CompileExpression<string>(exprB);

        // update a custom variable after compilation
        contextB.SetVariable("x", 5);

        // Call the delegates and write the results to the console
        Console.WriteLine("{0} -> {1}", exprA, funA(2));
        Console.WriteLine("{0} -> {1}", exprB, funB());

        // The output looks like this:
        // > sin(pi * neg(10.0 + x)) + a -> 2
        // > "High " & X & "!" -> High 5!
    }
}

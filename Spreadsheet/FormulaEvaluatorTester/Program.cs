// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
string expression = "( 2 + 35 ) * A7 + B88 * 4";
string[] splitExpression = FormulaEvaluator.Evaluator.SplitExpression(expression);
foreach (string item in splitExpression) {
    Console.WriteLine(item);
}

FormulaEvaluator.Evaluator.Evaluate(expression);


Console.Read();

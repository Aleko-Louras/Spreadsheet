///
/// A test program for the Evaluator class
///
/// 
///
string expression = "( 2 + 35 )"; // 37
string expression2 = "(2 + 3) / 2 + 2"; // 4
string expression3 = " 2/ 0"; // Error: Divide by Zero
string expression4 = ""; // Error: Empty/Bad Argument
string expression5 = "((2+2)+2)"; //6

//Test the expression splitter
string[] splitExpression = FormulaEvaluator.Evaluator.SplitExpression(expression);
foreach (string item in splitExpression) {
    //Console.WriteLine(item);
}

int result = FormulaEvaluator.Evaluator.Evaluate(expression5);
Console.WriteLine(result);

Console.Read();

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

// Lookup method

static int variableLookup(string v) {
    return 1;
}

string[] testExpressions = {
    "5 + 3", //8
    "4 * 7 - 2", // 26
    "9 / 3 + 2 * 8", // 19
    "(6 - 2) * 5", // 20
    "7 + 3 / 2", // 8
    "8 * (4 + 6)", // 80
    "(12 - 5) / 2", // 3
    "3 + 4 * (6 - 2)", //19
    "5 / 2 + 7 - 1", // 8
    "(9 + 3) / (4 - 1) * 2", //8
    "A7", //1
    "A99 + 3 * 4" //13

};

//Test the expression splitter
string[] splitExpression = FormulaEvaluator.Evaluator.SplitExpression(expression);
foreach (string item in splitExpression) {
    //Console.WriteLine(item);
}

foreach(string test in testExpressions) {
    Console.WriteLine(FormulaEvaluator.Evaluator.Evaluate(test, variableLookup));
}

Console.Read();

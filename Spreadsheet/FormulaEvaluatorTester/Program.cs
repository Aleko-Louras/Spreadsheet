
using static System.Net.Mime.MediaTypeNames;

///
/// A test program for the Evaluator class
///
/// 
///

// Lookup method for delegate
static int variableLookup(string v) {
    return 1;
}

string[] testExpressions = {
    // Addition, Subtraction, Multiplication and Division
    "7-24/8*4+6", // Expected: 1
    "18 / 3-7+2 * 5", // Expected:  9
    "6 * 4 / 12 + 72 / 8 - 9", // Expected: 2

    // Four operators and parenthesis
    "(17 - 6 / 2) + 4 * 3", // Expected: 26
    "7 + 3 / 2", // Expected: 8
    "8 * (4 + 6)", // Expected: 80 
    "(12 - 5) / 2", // Expected: 3
    "3 + 4 * (6 - 2)", //Expected: 19
    "5 / 2 + 7 - 1", // Expected: 8
    "(9 + 3) / (4 - 1) * 2", // Expected: 8

    //Expressions with variables
    "A7", // Expected: 1
    "A99 + 3 * 4", // Expected: 13
    "(2 + c6) * 5 + 2", // Expected: 17
    "(2 + AA6) * 5 + 2", // Expected: 17
    "5", // Expected: 5
    "(A6 * A6 + A6 * A6) * A6", // Expected: 2

    // Expressions with parens in the middle
    "7 * (6 / 2) + 9", // Expected: 30
    "4 * (2 + 3) / 2", // Expected: 10
    "3 * (2 + 4) - 7", // Expected: 11
    "((1 * 2 + (3*4) + (5*6)  +34  - 1 - 1 -1) /5) + AAABBBBCCCC1111222233334444 ", // Expected 16

    //Random
    "1 + (2)"

};
int[] answers = new int[21];

//Test valid expressions
for (int i = 0; i < testExpressions.Length; i++) {
    answers[i] = (FormulaEvaluator.Evaluator.Evaluate(testExpressions[i], variableLookup));
}

int[] myAnswers = { 1, 9, 2, 26, 8, 80, 3, 19, 8, 8, 1, 13, 17, 17, 5, 2, 30, 10, 11, 16,3 };

if(Enumerable.SequenceEqual(answers, myAnswers)) {
    Console.WriteLine("All valid expressions return the correct result");
} else {
    Console.WriteLine("TEST FAILED: NOT ALL VALID EXPRESSIONS RETURN CORRECLTY");
}




//Test invalid expressions
string[] invalidExpressions = new string[] {
    "(((((())))))",
    "()",
    "+",
    "A B C D",
    "1 2 3",
    "(",
    ")",
    "(1",
    "1)",
    "1+1)",
    "7 (5)",
    "6 + ( + 4)",
    "This is the way",
    ") + 5",
    "(5+5))",
    " 1/ 0",
    "0 / 0",
    " 1 + + 1",
    "-1 - 1",
    "1(2+3)"
};

int errorCounter = 0;
for (int i = 0; i < invalidExpressions.Length; i++) {
   try {
        int result = FormulaEvaluator.Evaluator.Evaluate(invalidExpressions[i], variableLookup);
    } catch (ArgumentException e) {
        errorCounter += 1;
    }
}

if (errorCounter == 20) {
    Console.WriteLine("All invalid expressions throw properly");
} else {
    Console.WriteLine("INVALID EXPRESSION TEST FAILED");
}


Console.Read();

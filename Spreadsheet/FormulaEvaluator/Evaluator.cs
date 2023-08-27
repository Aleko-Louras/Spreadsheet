using System.Text.RegularExpressions;

namespace FormulaEvaluator;

public static class Evaluator {
    public delegate int Lookup(String v);

    public static int Evaluate(String exp) {

        Stack<int> values = new Stack<int>();
        Stack<string> operators = new Stack<string>();

        string operatorSrings = "+-*-()";

        string[] tokens = SplitExpression(exp);

        foreach(string token in tokens) {
            if (String.IsNullOrEmpty(token)) {
                continue;
            }
        }

        return 0;
    }

    public static string[] SplitExpression(string exp) {

        string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

        for (int i = 0; i < substrings.Length; i++) {
            substrings[i] = substrings[i].Trim();
        }
        return substrings;
    }
    
}

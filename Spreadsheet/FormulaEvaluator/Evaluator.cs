///
/// Evaluator.cs
/// FormulaEvaluator
///
/// Created 8/27/23
///

using System.Text.RegularExpressions;

namespace FormulaEvaluator;

public static class Evaluator {
    public delegate int Lookup(String v);

    public static int Evaluate(String exp) { //,Lookup variableEvaluator

        Stack<int> values = new Stack<int>();
        Stack<string> operators = new Stack<string>();

        string[] tokens = SplitExpression(exp);

        foreach (string token in tokens) {
            if (String.IsNullOrEmpty(token)) {
                continue;
            }

            // Integer case 
            if (int.TryParse(token, out int intToken)) {
                if (values.Count > 0) {
                    string topOperator = operators.Peek();

                    if (topOperator == "*" || topOperator == "/") {
                        int topValue = values.Pop();
                        topOperator = operators.Pop();

                        int result = Operate(topValue, topOperator, intToken);
                        values.Push(result);
                    } else { values.Push(intToken); }
                }
                else { values.Push(intToken); }
            }

            // Variable case

            // + or - case

            else if (token == "+" || token == "-") {
                if (operators.Count > 0) {
                    string topOperator = operators.Peek();
                    if (topOperator == "+" || topOperator == "-") {
                        int value1 = values.Pop();
                        int value2 = values.Pop();

                        topOperator = operators.Pop();

                        int result = Operate(value1, topOperator, value2);
                        values.Push(result);

                        operators.Push(token);
                    }
                }
                operators.Push(token);
            }

            // * or / case
            else if (token == "*" || token == "/") {
                operators.Push(token);
            }

            else if (token == "(") {
                operators.Push(token);
            }
            else if (token == ")") {
                string topOperator = operators.Peek();
                if (topOperator == "+" || topOperator == "-") {
                    if (values.Count < 2) {

                    }

                    int value1 = values.Pop();
                    int value2 = values.Pop();
                    topOperator = operators.Pop();

                    int result = Operate(value1, topOperator, value2);

                    values.Push(result);

                    if (operators.Peek() == "("){
                        operators.Pop();
                    }
                    else {
                        throw new ArgumentException("Bad expression");
                    }
                }
                else if (topOperator == "*" || topOperator == "/") {

                    int value1 = values.Pop();
                    int value2 = values.Pop();
                    topOperator = operators.Pop();

                    int result = Operate(value1, topOperator, value2);

                    values.Push(result);
                }
            }      
        }

        if (operators.Count == 0) {
            return values.Pop();
        }
        else if (operators.Count == 1 && values.Count == 2) {
            int value1 = values.Pop();
            int value2 = values.Pop();
            string topOperator = operators.Pop();

            return Operate(value1, topOperator, value2);
        }
        else throw new Exception("Failed to evaluate expression");
    }

    public static string[] SplitExpression(string exp) {

        string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

        for (int i = 0; i < substrings.Length; i++) {
            substrings[i] = substrings[i].Trim();
        }
        return substrings;
    }

    static int Operate(int value1, string op, int value2) {


        switch (op) {
            case "+":
                return value1 + value2;
            case "-":
                return value1 - value2;
            case "*":
                return value1 * value2;
            case "/":
                if (value2 == 0) {
                    throw new ArgumentException("Illegal: division by zero");
                }
                return value1 / value2; //Integer division 
            default:
                throw new ArgumentException("Illegal operator");
        }
    }
    
}

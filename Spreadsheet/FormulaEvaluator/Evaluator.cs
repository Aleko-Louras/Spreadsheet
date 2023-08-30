///
/// Evaluator.cs
/// FormulaEvaluator
///
/// This class defines a method for evaluating simple infix
/// arithmetic expressions.
/// 
/// Created 8/27/23
///

using System.Text.RegularExpressions;

namespace FormulaEvaluator;

public static class Evaluator {
    public delegate int Lookup(String v);

    /// <summary>
    /// Evaluate returns an integer result for simple arithmetic expressions.
    /// The only legal tokens are the four operator symbols + - * /, left and
    /// right parenthesis, non-negtive integers, whitespace, and variables which
    /// consist of one or more letters followed by one or more digits. 
    /// </summary>
    /// <param name="exp"> The arithmetic expression</param>
    /// <param name="variableEvaluator"> A function for variable Evaluation</param>
    /// <returns> The integer result of the expression </returns>
    /// <exception cref="Exception"></exception>
    public static int Evaluate(String exp, Lookup variableEvaluator) { 

        Stack<int> values = new Stack<int>();
        Stack<string> operators = new Stack<string>();

        string[] tokens = SplitExpression(exp);
        if(tokens.Length < 1) {
            throw new ArgumentException("The expression must contain values");
        }

        foreach (string token in tokens) {
            
            if (String.IsNullOrEmpty(token)) {
                continue;
            }

            //Variable case, lookup the variable, the procede as in integer case.
            if (char.IsLetter(token[0])) {
                int varValue = variableEvaluator(token);
                
                if (values.Count > 0) { // Don't pop from an empty stack
                    string topOperator = operators.Peek();

                    if (topOperator == "*" || topOperator == "/") {
                        int topValue = values.Pop();
                        topOperator = operators.Pop();

                        int result = Operate(varValue, topOperator, topValue);
                        values.Push(result);
                    }
                    else { values.Push(varValue); }
                }
                else { values.Push(varValue); }
            }


            // Integer case 
            if (int.TryParse(token, out int intToken)) {
                if (values.Count > 0) {  // Don't pop from empty stack
                    string topOperator = operators.Peek();

                    if (topOperator == "*" || topOperator == "/") {
                        int topValue = values.Pop();
                        topOperator = operators.Pop();

                        int result = Operate(intToken, topOperator, topValue);
                        values.Push(result);
                    }
                    else { values.Push(intToken); }
                }
                else { values.Push(intToken); }
            }

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
                if (operators.Count > 0) {
                    string topOperator = operators.Peek();
                    if (topOperator == "+" || topOperator == "-") {

                        int value1 = values.Pop();
                        int value2 = values.Pop();
                        topOperator = operators.Pop();

                        int result = Operate(value1, topOperator, value2);

                        values.Push(result);

                        if (operators.Peek() == "(") {
                            operators.Pop();
                        }
                        if (operators.Count() > 0) {
                            topOperator = operators.Peek();
                            if (topOperator == "*" || topOperator == "/") {
                                value1 = values.Pop();
                                value2 = values.Pop();
                                topOperator = operators.Pop();

                                result = Operate(value1, topOperator, value2);

                                values.Push(result);
                            }
                        }

                    }
                    if (topOperator == "(") {
                        operators.Pop();
                        if (operators.Count > 0) {
                            topOperator = operators.Peek();
                        }
                    }
                    if (operators.Count > 0) {
                        if (topOperator == "*" || topOperator == "/") {

                            int value1 = values.Pop();
                            int value2 = values.Pop();
                            topOperator = operators.Pop();

                            int result = Operate(value1, topOperator, value2);

                            values.Push(result);
                        }
                    }
                }
            }      
        }

        if (operators.Count == 0) {
            if (values.Count == 1) {
                return values.Pop();
            } else {
                throw new ArgumentException("Failed to evaluate expression: Did you enter a valid expression?");
            }
        }
        else if (operators.Count == 1 && values.Count == 2) {
            int value1 = values.Pop();
            int value2 = values.Pop();
            string topOperator = operators.Pop();

            return Operate(value1, topOperator, value2);
        }
        else throw new ArgumentException("Failed to evaluate expression: Did you enter a valid expression?");
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
                return value2 - value1;
            case "*":
                return value1 * value2;
            case "/":
                if (value1 == 0) {
                    throw new ArgumentException("Illegal: division by zero");
                }
                return value2 / value1; //Integer division 
            default:
                throw new ArgumentException("Illegal operator");
        }
    }
    
}

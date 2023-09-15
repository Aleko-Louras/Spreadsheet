// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)
//
// Implementation by Quinn Pritchett
// September 2023
//

using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula {

    private readonly List<string> variables = new();
    // This structure holds the formula with valid syntax, and normalized vars
    private readonly List<string> normalizedTokens = new();

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true) {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid) {

        IEnumerable<string> enumerableTokens = GetTokens(formula);
        List<string> tokens = enumerableTokens.ToList();

        AddTokensandNormalizeVariables(normalize, isValid, tokens);
        FormulaHasValidSyntax(normalizedTokens);
        FormulaHasValidTokens(normalizedTokens);
        
    }

    

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup) {

        Stack<double> values = new Stack<double>();
        Stack<string> operators = new Stack<string>();


        foreach (string token in normalizedTokens) {

            //Variable case, lookup the variable, then procede as in integer case.
            if (IsVariable(token)) {
                double varValue = lookup(token); //Variable evaluation is handled by delegated function

                if (operators.Count > 0) {
                    string topOperator = operators.Peek();

                    if (isMultiplyOrDivide(topOperator)) {
                        if (values.Count >= 1) {
                            double topValue = values.Pop();
                            topOperator = operators.Pop();
                            try {
                                double result = HalfOperate(varValue, topOperator, topValue);
                                values.Push(result);
                            }
                            catch(ArgumentException) {
                                return new FormulaError();
                            }
                            
                        }
                    }
                    else { values.Push(varValue); }
                }
                else { values.Push(varValue); }
            }


            // Integer case 
            if (double.TryParse(token, out double intToken)) {
                if (operators.Count > 0) {
                    string topOperator = operators.Peek();

                    if (isMultiplyOrDivide(topOperator)) {
                        if (values.Count >= 1) {
                            double topValue = values.Pop();
                            topOperator = operators.Pop();

                            try {
                                double result = HalfOperate(intToken, topOperator, topValue);
                                values.Push(result);
                            }
                            catch (ArgumentException) {
                                return new FormulaError("Error: Division by Zero");
                            }
                        }
                    }
                    else { values.Push(intToken); }
                }
                else { values.Push(intToken); }
            }

            // Additon or subtraction case
            else if (isAddOrSubtract(token)) {
                if (operators.Count > 0) {
                    string topOperator = operators.Peek();
                    if (isAddOrSubtract(topOperator)) {
                        if (values.Count >= 2) {
                            try {
                                double result = FullOperate(values, operators);
                                values.Push(result);
                            }
                            catch(ArgumentException) {
                                return new FormulaError("Error: Division by Zero");
                            }
                        }
                    }
                }
                operators.Push(token);
            }

            // Multiplication or division case
            else if (isMultiplyOrDivide(token)) {
                operators.Push(token);
            }

            // Left parenthesis case
            else if (isOpenParen(token)) {
                operators.Push(token);
            }

            //Right parenthesis case
            else if (token == ")") {
                if (operators.Count > 0) {
                    string topOperator = operators.Peek();
                    if (isAddOrSubtract(topOperator)) {
                        if (values.Count >= 2) {

                            try {
                                double result = FullOperate(values, operators);
                                values.Push(result);
                            }
                            catch (ArgumentException) {
                                return new FormulaError("Error: Division by Zero");
                            }

                            if (operators.Count > 0) {
                                topOperator = operators.Peek();
                            }
                        }
                    }

                    if (isOpenParen(topOperator)) {
                        operators.Pop();
                        if (operators.Count > 0) {
                            topOperator = operators.Peek();
                        }
                    }
                    else { throw new ArgumentException(); }
                    if (operators.Count > 0) {
                        if (isMultiplyOrDivide(topOperator)) {
                            if (values.Count >= 2) {
                                try {
                                    double result = FullOperate(values, operators);
                                    values.Push(result);
                                }
                                catch (ArgumentException) {
                                    return new FormulaError("Error: Division by Zero");
                                }
                            }
                        }
                    }
                }
            }
        }

        // Once the last token has been processed, validate the curent state
        if (operators.Count == 0) {
            return values.Pop();

        }
        else if (operators.Count == 1 && values.Count == 2) {
            if (operators.Peek() == "+" || operators.Peek() == "-") {
                return FullOperate(values, operators);

            }
            else return new FormulaError();
        }
        else return new FormulaError();

    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables() {
        return variables;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString() {
        string formulaString = string.Join("", normalizedTokens);
        return formulaString;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj) {
        if (obj == null || obj is not Formula) return false;

        if (obj.ToString() == this.ToString()) {
            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2) {

        if (f1.Equals(f2)) {
            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2) {

        if (f1.Equals(f2)) {
            return false;
        }
        else {
            return true;
        }
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode() {
        int hashCode = this.ToString().GetHashCode();
        return hashCode;
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula) {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace)) {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline)) {
                yield return s;
            }
        }

    }
    /// <summary>
    /// If normalized variables are valid, they are added to the normalizedTokens
    /// and the varialbes lists. If the variable not in the variables list it is
    /// added otherwise it is not added. Numbers are parsed as doubles and added
    /// to the normalizedTokens list. Other tokens are added. 
    /// </summary>
    /// <param name="normalize"> the Normalizer </param>
    /// <param name="isValid"> the Validator </param>
    /// <param name="tokens"> the list of tokens </param>
    private void AddTokensandNormalizeVariables(Func<string, string> normalize, Func<string, bool> isValid, List<string> tokens) {
        foreach (string token in tokens) {
            if (IsVariable(token)) {
                if (isValid(normalize(token))) {
                    normalizedTokens.Add(normalize(token));

                    if (!variables.Contains(normalize(token))) {
                        variables.Add(normalize(token));
                    }
                }
            }
            else if (IsNumber(token)) {
                Double d = Double.Parse(token);
                string s = d.ToString();
                normalizedTokens.Add(s);
            }
            else {
                normalizedTokens.Add(token);
            }
        }
    }

    /// <summary>
    /// Returns true if the list of tokens are all valid
    /// Otherwise throws a Formula Format Exception taht
    /// an invalid token was found 
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    /// <exception cref="FormulaFormatException"></exception>
    private static bool FormulaHasValidTokens(List<string> tokens) {
        // Parsing
        foreach (string token in tokens) {
            if (IsOperator(token) ||
                IsVariable(token) ||
                IsNumber(token) ||
                IsOpenParen(token) ||
                IsCloseParen(token)) {
                continue;
            }
            else {
                throw new FormulaFormatException("Formula Format Exception: Invalid token found, check formula");
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true if the Formula has valid syntax.
    /// Otherwise throws a Formula Format Exception. 
    /// </summary>
    /// <param name="tokens"> The formula to check </param>
    /// <returns></returns>
    /// <exception cref="FormulaFormatException"></exception>
    private static bool FormulaHasValidSyntax(List<string> tokens) {

        // One token rule
        if (tokens.Count == 0) {
            throw new FormulaFormatException(
                "Formula Format Exception: There must be at least one token");
        }
        

        // Right Parent and Balanced Paren Rules
        int numRP = 0;
        int numLP = 0;
        foreach (string token in tokens) {

            if (token == "(")
                numRP += 1;
            if (token == ")") {
                numLP += 1;

                if (numLP > numRP) {
                    throw new FormulaFormatException(
                        "Formula Format Exception: Unmatched left parenthesis"
                        );
                }
            }
        }

        if (numLP != numRP) {
            throw new FormulaFormatException(
                "Formula Format Exeption: Unbalanced Parentheses");
        }
        // Starting Token Check
        string firstElement = tokens.ElementAt(0);
        if (!(IsNumber(firstElement) || IsOpenParen(firstElement) || IsVariable(firstElement))) {
            throw new FormulaFormatException(
                "Formula Format Exception: The first token of an expression must be a" +
                " number, a variable, or an opening parenthesis.");
        }

        // Ending Token Check
        string lastElement = tokens.Last();
        if (!(IsNumber(lastElement) || IsVariable(lastElement) || IsCloseParen(lastElement))) {
            throw new FormulaFormatException(
                "Formula Format Exception: The last token of an expression must be a" +
                " number, a variable, or a closing parenthesis.");
        }


        // Parenthesis/Operator Following Rule
        for (int i = 0; i < tokens.Count; i++) {
            if (IsOpenParen(tokens[i]) || IsOperator(tokens[i])) {
                string followingToken = tokens[i + 1];

                if (!(IsNumber(followingToken) || IsOpenParen(followingToken) || IsVariable(followingToken))) {
                    throw new FormulaFormatException(
                        "Formula Format Exception: Any token that immediately " +
                        "follows an opening parenthesis or an operator must be " +
                        "either a number, a variable, or an opening parenthesis.");
                }
            }
        }

        // Extra Following Rule
        for (int i = 0; i < tokens.Count; i++) {
            if (tokens.Count == 1) break;
            if (i == tokens.Count - 1) break;

            if (IsNumber(tokens[i]) || IsVariable(tokens[i]) || IsCloseParen(tokens[i])) {
                string followingToken = tokens[i + 1];

                if (!(IsOperator(followingToken) || IsCloseParen(followingToken))) {
                    throw new FormulaFormatException(
                        "Formula Format Exception: Any token that immediately " +
                        "follows a number, a variable, or a closing parenthesis " +
                        "must be either an operator or a closing parenthesis.");
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true if the string matches the number pattern.
    /// Otherwise returns false. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool IsNumber(string token) {
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        if (Regex.IsMatch(token, doublePattern, RegexOptions.IgnorePatternWhitespace)) {
            return true;
        }
        else return false;
    }
    /// <summary>
    /// Returns true if the string matches the variable pattern.
    /// If the varialbe is recognized as a number it returns false. 
    /// Otherwise returns false. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool IsVariable(string token) {
        if (Double.TryParse(token, out double d)) {
            return false;
        }
        
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        if (Regex.IsMatch(token, varPattern)) {
            return true;
        }
        else return false;
    }
    /// <summary>
    /// Returns true if the string matches the operator pattern. 
    /// Otherwise returns false. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool IsOperator(string token) {
        string opPattern = @"[\+\-*/]";
        if (Regex.IsMatch(token, opPattern)) {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Returns true if the string matches an open paren
    /// Otherwise returns false. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool IsOpenParen(string token) {
        string lpPattern = @"\(";
        if (Regex.IsMatch(token, lpPattern)) {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Returns true if the string matches the number pattern.
    /// Otherwise returns false. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool IsCloseParen(string token) {
        string rpPattern = @"\)";
        if (Regex.IsMatch(token, rpPattern)) {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Applies an operator string to the two provided values
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="op"></param>
    /// <param name="value2"></param>
    /// <returns>The integer result of the operation</returns>
    /// <exception cref="ArgumentException"></exception>
    private static double HalfOperate(double value1, string op, double value2) {

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
                return value2 / value1; 
            default:
                throw new ArgumentException();
        }
    }
    /// <summary>
    /// Applies the given operator to top to values in the provided stack
    /// </summary>
    /// <param name="values"> The stack of values</param>
    /// <param name="operators">The operation to be performed </param>
    /// <returns>The integer result of the operation</returns>
    /// <exception cref="ArgumentException"></exception>
    private static double FullOperate(Stack<double> values, Stack<string> operators) {
        double value1 = values.Pop();
        double value2 = values.Pop();

        string topOperator = operators.Pop();

        switch (topOperator) {
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
                return value2 / value1; 
            default:
                throw new ArgumentException();
        }
    }

    
    /// <summary>
    /// Returns true if the given string is a "*" or a "/"
    /// </summary>
    /// <param name="token"> the string </param>
    /// <returns>A boolean result</returns>
    private static bool isMultiplyOrDivide(string token) {
        if (token == "*" || token == "/") {
            return true;
        }
        else {
            return false;
        }
    }
    /// <summary>
    /// Returns true if the given string is a "("
    /// </summary>
    /// <param name="token"> The string </param>
    /// <returns>A boolean result</returns>
    private static bool isOpenParen(string token) {
        if (token == "(") {
            return true;
        }
        else {
            return false;
        }
    }
    /// <summary>
    /// Returns true if the given string is a "+" or a "-"
    /// </summary>
    /// <param name="token"> The string</param>
    /// <returns>A boolean result</returns>
    private static bool isAddOrSubtract(string token) {
        if (token == "+" || token == "-") {
            return true;
        }
        else {
            return false;
        }
    }
}


/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception {
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message) {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError {
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this() {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

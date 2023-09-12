namespace FormulaTests;
using Newtonsoft.Json.Linq;

using System.Runtime.InteropServices;
using SpreadsheetUtilities;

[TestClass]
public class FormulaTests {

    // Lookup method for delegate
    static int variableLookup(string v) {
        return 1;
    }

    static void normalize(string v) {

    }

    // Only valid tokens should be in the formula
    [TestMethod]
    public void ParsingRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("@"); });
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("(16 + 17) + A$"); });

    }

    // There must be at least one token.
    [TestMethod]
    public void OneTokenRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula(""); });

    }

    // When reading tokens from left to right, at no point should the number of
    // closing parentheses seen so far be greater than the number of opening
    // parentheses seen so far.
    [TestMethod]
    public void RightParenthesesRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("())"); });

    }

    // The total number of opening parentheses must equal the total number of
    // closing parentheses.
    [TestMethod]
    public void BalancedParenthesesRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("(((5))"); });

    }

    // The first token of an expression must be a number, a variable,
    // or an opening parenthesis.
    [TestMethod]
    public void StartingTokenRule() {
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula(")"); });
    }

    // The last token of an expression must be a number, a variable, or a closing parenthesis.
    [TestMethod]
    public void EndingTokenRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("(+"); });

    }

    // Any token that immediately follows an opening parenthesis or an operator
    // must be either a number, a variable, or an opening parenthesis.
    [TestMethod]
    public void ParenFollowingRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("(+"); });

    }

    // Any token that immediately follows a number, a variable, or a closing
    // parenthesis must be either an operator or a closing parenthesis.
    [TestMethod]
    public void ExtraFollowingRule() {

        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("4("); });
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("4 E"); });

    }
}

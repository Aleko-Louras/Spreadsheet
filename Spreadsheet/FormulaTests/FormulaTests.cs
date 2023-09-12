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

    static string N(string v) {
        return v.ToUpper();
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
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("("); });

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

    [TestMethod]
    public void GetNoVariablesTest() {
        Formula f = new Formula("1");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    [TestMethod]
    public void GetOneVariableTest() {
        Formula f = new Formula("A1");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        string s1 = e.Current;
        Assert.AreEqual("A1", s1);
    }

    [TestMethod]
    public void GetVariablesNoNormalizationTest() {
        Formula f = new Formula("x+X*z");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s3 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "x") && (s2 == "X")) && (s3 == "z"));
    }

    // Should enumerate X,Y,Z
    [TestMethod]
    public void GetVariablesWithNormalizationTest() {
        Formula f = new Formula("x+y*z", N, s => true);
        IEnumerator<string> e = f.GetVariables().GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s3 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "X") && (s2 == "Y")) && (s3 == "Z"));
    }

    [TestMethod]
    public void GetVariablesWithDuplicationNormalizationTest() {
        Formula f = new Formula("x+X*z", N, s => true);
        IEnumerator<string> e = f.GetVariables().GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "X") && (s2 == "Z")));
    }
}

namespace SpreadsheetTests;

using SS;
using SpreadsheetUtilities;

[TestClass]
public class SpreadsheetUnitTests {

    [TestMethod]
    public void EmptySpreadsheetHasNoNamedCellsTest() {
        Spreadsheet s = new Spreadsheet();

        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    [TestMethod]
    public void GetNamesOfAllNonemptyCellsTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", 1.0);
        s.SetCellContents("A2", new Formula("1+1"));

        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        string name1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        string name2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((name1 == "A1") && (name2 == "A2")) ||
                      ((name1 == "A2") && (name2 == "A1")));
    }

    [TestMethod]
    public void GetCellContentsAsNumberTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A2", 2.0);
        double d = 2.0;
        Assert.IsTrue(d.Equals(s.GetCellContents("A2")));

    }

    [TestMethod]
    public void GetCellContentsAsTextTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A3", "Hello");
        string hello = "Hello";
        Assert.IsTrue(hello.Equals(s.GetCellContents("A3")));

    }

    [TestMethod]
    public void GetCellContentsAsFormulaTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A1", new Formula("1+1"));
        Formula f = new Formula("1+1");
        Assert.IsTrue(f.Equals(s.GetCellContents("A1")));
    }

    [TestMethod]
    public void SetCellContentsWithNumberTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetCellContents("A1", 1.0).ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    [TestMethod]
    public void SetCellContentsWithTextTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetCellContents("A1", "My Cell Name").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    [TestMethod]
    public void SetCellContentsWithFormulaTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetCellContents("A1", new Formula("1+1")).ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    [TestMethod]
    public void GetDirectDependentsTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A1", 3);
        s.SetCellContents("B1", new Formula("A1 * A1"));
        s.SetCellContents("C1", new Formula("B1 + A1"));
        s.SetCellContents("D1", new Formula("B1 - C1"));

        IEnumerator<string> e = s.GetDirectDependents("A1").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        string name1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        string name2 = e.Current;
        Assert.IsFalse(e.MoveNext());

        Assert.IsTrue(((name1 == "B1") && (name2 == "C1")) ||
                      ((name1 == "C1") && (name2 == "B1")));
    }
}

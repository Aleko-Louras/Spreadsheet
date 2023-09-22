namespace SpreadsheetTests;

using SS;
using SpreadsheetUtilities;

[TestClass]
public class SpreadsheetUnitTests {

    /// <summary>
    /// Creating a new spreadsheet
    /// </summary>
    [TestMethod]
    public void AbstractSpreadsheetTest() {
        AbstractSpreadsheet sheet = new Spreadsheet();
    }

    /// <summary>
    /// A new spreadsheet should have no named cells 
    /// </summary>
    [TestMethod]
    public void EmptySpreadsheetHasNoNamedCellsTest() {
        Spreadsheet s = new Spreadsheet();

        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    /// If cells are given names, there should be two cells in the
    /// list of nonempty cells 
    /// </summary>
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

    /// <summary>
    /// A test to get doubles out of the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAsNumberTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A2", 2.0);
        double d = 2.0;
        Assert.IsTrue(d.Equals(s.GetCellContents("A2")));

    }

    /// <summary>
    /// A test to get text out of the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAsTextTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A3", "Hello");
        string hello = "Hello";
        Assert.IsTrue(hello.Equals(s.GetCellContents("A3")));

    }

    /// <summary>
    /// A test to get a Formula out of the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAsFormulaTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A1", new Formula("1+1"));
        Formula f = new Formula("1+1");
        Assert.IsTrue(f.Equals(s.GetCellContents("A1")));
    }

    /// <summary>
    /// If a cell is empty, it should return an empty string
    /// </summary>
    [TestMethod]
    public void GetCellContentsOfEmptyCellTest() {
        Spreadsheet s = new Spreadsheet();
        string empty = "";
        Assert.IsTrue(empty.Equals(s.GetCellContents("A1")));
    }

    /// <summary>
    /// If a spreadsheet has onecell has a name but is later emptied there
    /// should then be no named cells in the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAfterEmptyingTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", "hello");
        Assert.AreEqual("hello", s.GetCellContents("A1"));
        s.SetCellContents("A1", "");
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// </summary>
    [TestMethod]
    public void GetCellContentsInvalidName() {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => {
            s.GetCellContents("!^A1");
        });
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// This tests the number overload 
    /// </summary>
    [TestMethod]
    public void SetCellContentsToNumberWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetCellContents("!^A1", 5);
        });
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// This tests the string overload 
    /// </summary>
    [TestMethod]
    public void SetCellContentsToStringWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetCellContents("$^A1", "hello");
        });
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// This tests the Formula overload 
    /// </summary>
    [TestMethod]
    public void SetCellContentsToFormulaWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetCellContents("#^A1", new Formula("1+1"));
        });
    }

    /// <summary>
    /// Tests setting a cell with a number
    /// Should return just the name of the cell
    /// </summary>
    [TestMethod]
    public void SetCellContentsWithNumberTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetCellContents("A1", 1.0).ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// Tests setting a cell with text
    /// Should return just the name of the cell 
    /// </summary>
    [TestMethod]
    public void SetCellContentsWithTextTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetCellContents("A1", "My Cell Name").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// Tests setting a cell with a Formula
    /// Should return just the name of the cell
    /// </summary>
    [TestMethod]
    public void SetCellContentsWithFormulaTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetCellContents("A1", new Formula("1+1")).ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// If the cell contents are set with a dependency
    /// Set cell contents should return name, plus all dependencies 
    /// </summary>
    [TestMethod]
    public void SetCellContentsAfterSetupTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("b1", new Formula("8-a1"));
        s.SetCellContents("c1", new Formula("a1"));

        List<string> expected =  new List<string> { "a1", "b1", "c1" };
        List<string> result = s.SetCellContents("a1", 10.9).ToList();
        CollectionAssert.AreEquivalent(expected, result); 

    }

    /// <summary>
    /// Setting cell contents with nested dependencies 
    /// </summary>
    [TestMethod]
    public void SetCellContentsNestedDependencyTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", new Formula("15.0"));
        s.SetCellContents("b1", new Formula("5.0"));
        s.SetCellContents("c1", new Formula("a1 + b1"));
        s.SetCellContents("d1", new Formula("c1 + 4"));
        s.SetCellContents("e1", new Formula("d1+ c1"));

        List<string> expected = new List<string> { "a1", "c1", "d1", "e1" };
        List<string> result = s.SetCellContents("a1", 20).ToList();
        CollectionAssert.AreEquivalent(expected, result);

    }

    /// <summary>
    /// If a Formula cell in the middle of a dependency chain
    /// is set to a number, it no longer has any dependees. 
    /// </summary>
    [TestMethod]
    public void ReplacingTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A1", 3);
        s.SetCellContents("B1", new Formula("A1 + 3"));
        s.SetCellContents("C1", new Formula("B1 + 1"));

        // Replace a middle node with a number
        // It now is no longer dependent on any other cells
        // It may still have dependents
        s.SetCellContents("B1", 4);

        // Cell A1 no longer has any direct dependents
        // So if we set its value, the returned list should
        // only contain A1
        List<string> result = s.SetCellContents("A1", "1").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// If a Formula cell in the middle of a dependency chain
    /// is set to a number, it no longer has any dependees. 
    /// </summary>
    [TestMethod]
    public void ReplacingTestB() {
        Spreadsheet s = new Spreadsheet();

        s.SetCellContents("A1", 3);
        s.SetCellContents("A2", 2);
        s.SetCellContents("B1", new Formula("A1 + 3"));
        s.SetCellContents("C1", new Formula("B1 + 1"));

        // Replace a middle node with a formula 
        List<string> cellsToRecalculate = s.SetCellContents("B1", new Formula("A2+A2")).ToList();
        Assert.AreEqual(2, cellsToRecalculate.Count);
        Assert.AreEqual("B1", cellsToRecalculate[0]);
        Assert.AreEqual("C1", cellsToRecalculate[1]);

    }

    /// <summary>
    /// If a circular dependency is created by SetCellContents
    /// it should throw a Circular Exception
    /// </summary>
    [TestMethod]
    public void CreateCircularDependency() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("B1"));
        Assert.ThrowsException<CircularException>(() => s.SetCellContents("B1", new Formula("A1")));

    }

    /// <summary>
    /// /// If a circular dependency is created by SetCellContents
    /// it should throw a Circular Exception
    ///
    /// This tests a larger circular dependency
    /// </summary>
    [TestMethod]
    public void CreateLargeCircularDependency() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("B1"));
        s.SetCellContents("B1", new Formula("C1"));
        s.SetCellContents("C1", new Formula("D1"));
        
        Assert.ThrowsException<CircularException>(() => s.SetCellContents("D1", new Formula("A1")));

    }

    /// <summary>
    /// If the contents of several cells are set to formulas
    /// then reset back to empty, there should be no names returned
    /// by GetNamesOfAllNonemptyCells.
    /// </summary>
    [TestMethod]
    public void SetAllCellsToEmptyCell() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("6"));
        s.SetCellContents("B1", new Formula("A1+1"));
        s.SetCellContents("C1", new Formula("B1+1"));
        s.SetCellContents("D1", new Formula("C1+1"));
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        s.SetCellContents("A1", "");
        s.SetCellContents("B1", "");
        s.SetCellContents("C1", "");
        s.SetCellContents("D1", "");
        IEnumerator<string> f = s.GetNamesOfAllNonemptyCells().GetEnumerator();

        Assert.IsFalse(f.MoveNext());

    }
}

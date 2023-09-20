namespace SpreadsheetTests;

using SS;
using SpreadsheetUtilities;

[TestClass]
public class SpreadsheetUnitTests {

    [TestMethod]
    public void AbstractSpreadsheetTest() {
        AbstractSpreadsheet sheet = new Spreadsheet();
    }

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
    public void GetCellContentsOfEmptyCellTest() {
        Spreadsheet s = new Spreadsheet();
        string empty = "";
        Assert.IsTrue(empty.Equals(s.GetCellContents("A1")));
    }

    [TestMethod]
    public void GetCellContentsAfterEmptyingTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", "hello");
        Assert.AreEqual("hello", s.GetCellContents("A1"));
        s.SetCellContents("A1", "");
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    [TestMethod]
    public void GetCellContentsInvalidName() {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => {
            s.GetCellContents("!^A1");
        });
    }

    [TestMethod]
    public void SetCellContentsToNumberWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetCellContents("!^A1", 5);
        });
    }

    [TestMethod]
    public void SetCellContentsToStringWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetCellContents("$^A1", "hello");
        });
    }

    [TestMethod]
    public void SetCellContentsToFormulaWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetCellContents("#^A1", new Formula("1+1"));
        });
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
    public void ReplacingTest() {
        Spreadsheet s = new Spreadsheet();

        //Set up initial graph
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

    [TestMethod]
    public void ReplacingTestB() {
        Spreadsheet s = new Spreadsheet();

        //Set up initial graph
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

    [TestMethod]
    public void ReplacingTestC() {
        Spreadsheet s = new Spreadsheet();

        //Set up initial graph
        s.SetCellContents("A1", 5);
        s.SetCellContents("B1", new Formula("A1 + 2"));
        s.SetCellContents("C1", new Formula("A1 + B1"));
        s.SetCellContents("D1", new Formula("A1 * 7"));

        // Replace A1 with 10
        IEnumerator<string> e = s.SetCellContents("A1", 10).GetEnumerator();

        List<List<string>> validOrders = new List<List<string>>
        {
            new List<string> { "A1", "B1", "C1", "D1" },
            new List<string> { "A1", "B1", "D1", "C1" },
            new List<string> { "A1", "D1", "B1", "C1" },
        };

        // Compare order to possible valid orders
        bool validOrderFound = false;
        foreach (var expectedOrder in validOrders) {
            IEnumerable<string> actualOrder = s.SetCellContents("A1", 10);
            if (expectedOrder.SequenceEqual(actualOrder)) {
                validOrderFound = true;
                break;
            }
        }

        Assert.IsTrue(validOrderFound);
    }

    [TestMethod]
    public void CreateCircularDependency() {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("B1"));
        Assert.ThrowsException<CircularException>(() => s.SetCellContents("B1", new Formula("A1")));

    }

    //[TestMethod]
    //public void GetDirectDependentsTest() {
    //    Spreadsheet s = new Spreadsheet();

    //    s.SetCellContents("A1", 3);
    //    s.SetCellContents("B1", new Formula("A1 * A1"));
    //    s.SetCellContents("C1", new Formula("B1 + A1"));
    //    s.SetCellContents("D1", new Formula("B1 - C1"));

    //    IEnumerator<string> e = s.GetDirectDependents("A1").GetEnumerator();
    //    Assert.IsTrue(e.MoveNext());
    //    string name1 = e.Current;
    //    Assert.IsTrue(e.MoveNext());
    //    string name2 = e.Current;
    //    Assert.IsFalse(e.MoveNext());

    //    Assert.IsTrue(((name1 == "B1") && (name2 == "C1")) ||
    //                  ((name1 == "C1") && (name2 == "B1")));
    //}
}

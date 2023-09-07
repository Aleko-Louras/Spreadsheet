using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace DevelopmentTests;

/// <summary>
///This is a test class for DependencyGraphTest and is intended
///to contain all DependencyGraphTest Unit Tests (once completed by the student)
///</summary>
[TestClass()]
public class DependencyGraphTest {

    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyTest() {
        DependencyGraph t = new DependencyGraph();
        Assert.AreEqual(0, t.NumDependencies);
    }


    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyRemoveTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(1, t.NumDependencies);
        t.RemoveDependency("x", "y");
        Assert.AreEqual(0, t.NumDependencies);
    }


    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void EmptyEnumeratorTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
        Assert.IsTrue(e1.MoveNext());
        Assert.AreEqual("x", e1.Current);
        IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
        Assert.IsTrue(e2.MoveNext());
        Assert.AreEqual("y", e2.Current);
        t.RemoveDependency("x", "y");
        Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
        Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
    }


    /// <summary>
    ///Replace on an empty DG shouldn't fail
    ///</summary>
    [TestMethod()]
    public void SimpleReplaceTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(t.NumDependencies, 1);
        t.RemoveDependency("x", "y");
        t.ReplaceDependents("x", new HashSet<string>());
        t.ReplaceDependees("y", new HashSet<string>());
    }



    ///<summary>
    ///It should be possibe to have more than one DG at a time.
    ///</summary>
    [TestMethod()]
    public void StaticTest() {
        DependencyGraph t1 = new DependencyGraph();
        DependencyGraph t2 = new DependencyGraph();
        t1.AddDependency("x", "y");
        Assert.AreEqual(1, t1.NumDependencies);
        Assert.AreEqual(0, t2.NumDependencies);
    }




    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void SizeTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");
        Assert.AreEqual(4, t.NumDependencies);
    }


    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void EnumeratorTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        // This is one of several ways of testing whether your IEnumerable
        // contains the right values. This does not require any particular
        // ordering of the elements returned.
        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }


    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void ReplaceThenEnumerate() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "b");
        t.AddDependency("a", "z");
        t.ReplaceDependents("b", new HashSet<string>());
        t.AddDependency("y", "b");
        t.ReplaceDependents("a", new HashSet<string>() { "c" });
        t.AddDependency("w", "d");
        t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
        t.ReplaceDependees("d", new HashSet<string>() { "b" });

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }



    /// <summary>
    ///Using lots of data
    ///</summary>
    [TestMethod()]
    public void StressTest() {
        // Dependency graph
        DependencyGraph t = new DependencyGraph();

        // A bunch of strings to use
        const int SIZE = 200;
        string[] letters = new string[SIZE];
        for (int i = 0; i < SIZE; i++) {
            letters[i] = ("" + (char)('a' + i));
        }

        // The correct answers
        HashSet<string>[] dents = new HashSet<string>[SIZE];
        HashSet<string>[] dees = new HashSet<string>[SIZE];
        for (int i = 0; i < SIZE; i++) {
            dents[i] = new HashSet<string>();
            dees[i] = new HashSet<string>();
        }

        // Add a bunch of dependencies
        for (int i = 0; i < SIZE; i++) {
            for (int j = i + 1; j < SIZE; j++) {
                t.AddDependency(letters[i], letters[j]);
                dents[i].Add(letters[j]);
                dees[j].Add(letters[i]);
            }
        }

        // Remove a bunch of dependencies
        for (int i = 0; i < SIZE; i++) {
            for (int j = i + 4; j < SIZE; j += 4) {
                t.RemoveDependency(letters[i], letters[j]);
                dents[i].Remove(letters[j]);
                dees[j].Remove(letters[i]);
            }
        }

        // Add some back
        for (int i = 0; i < SIZE; i++) {
            for (int j = i + 1; j < SIZE; j += 2) {
                t.AddDependency(letters[i], letters[j]);
                dents[i].Add(letters[j]);
                dees[j].Add(letters[i]);
            }
        }

        // Remove some more
        for (int i = 0; i < SIZE; i += 2) {
            for (int j = i + 3; j < SIZE; j += 3) {
                t.RemoveDependency(letters[i], letters[j]);
                dents[i].Remove(letters[j]);
                dees[j].Remove(letters[i]);
            }
        }

        // Make sure everything is right
        for (int i = 0; i < SIZE; i++) {
            Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
            Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
        }
    }

    //Begin own tests

    [TestMethod()]
    public void SimpleNumDependenciesTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");

        Assert.AreEqual(3, t.NumDependencies);
    }

    [TestMethod()]
    public void ReversedDependenciesAreDifferent() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("b", "a");

        Assert.AreEqual(2, t.NumDependencies);
        Assert.IsTrue(t.HasDependees("a"));
        Assert.IsTrue(t.HasDependees("b"));

        Assert.IsTrue(t.HasDependents("a"));
        Assert.IsTrue(t.HasDependents("b"));

        IEnumerator<string> e1 = t.GetDependees("a").GetEnumerator();
        Assert.IsTrue(e1.MoveNext());
        Assert.AreEqual("b", e1.Current);

        IEnumerator<string> e2 = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e2.MoveNext());
        Assert.AreEqual("a", e2.Current);
    }

    [TestMethod()]
    public void SimpleNumDependeesTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");
        t.AddDependency("b", "e");

        Assert.AreEqual(0, t.NumDependees("a"));
        Assert.AreEqual(1, t.NumDependees("b"));
        Assert.AreEqual(1, t.NumDependees("c"));
        Assert.AreEqual(1, t.NumDependees("d"));
        Assert.AreEqual(1, t.NumDependees("e"));
    }

    [TestMethod()]
    public void SimpleHasDependentsTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");
        t.AddDependency("b", "e");

        Assert.IsTrue( t.HasDependents("a"));
        Assert.IsTrue(t.HasDependents("b"));
        Assert.IsFalse(t.HasDependents("c"));
        Assert.IsFalse(t.HasDependents("d"));
        Assert.IsFalse(t.HasDependents("e"));
    }

    [TestMethod()]
    public void SimpleHasDependeesTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");
        t.AddDependency("b", "e");

        Assert.IsFalse(t.HasDependees("a"));
        Assert.IsTrue(t.HasDependees("b"));
        Assert.IsTrue(t.HasDependees("c"));
        Assert.IsTrue(t.HasDependees("d"));
        Assert.IsTrue(t.HasDependees("e"));


    }

    [TestMethod()]
    public void SimpleGetDependeesTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");
        t.AddDependency("b", "e");

        CollectionAssert.AreEqual(new[] { "b" }, t.GetDependees("e").ToList());
        CollectionAssert.AreEqual(new[] { "a" }, t.GetDependees("b").ToList());
        CollectionAssert.AreEqual(new[] { "a" }, t.GetDependees("c").ToList());
        CollectionAssert.AreEqual(new[] { "a" }, t.GetDependees("d").ToList());


    }
    [TestMethod()]
    public void ComplexGraphDepencencyTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("b", "d");
        t.AddDependency("c", "d");
        t.AddDependency("d", "e");
        t.AddDependency("e", "f");
        t.AddDependency("f", "g");
        t.AddDependency("g", "h");
        t.AddDependency("i", "j");
        t.AddDependency("j", "k");

        Assert.AreEqual(10, t.NumDependencies);
        Assert.IsFalse(t.HasDependees("a"));
        Assert.IsFalse(t.HasDependents("k"));


    }

    [TestMethod()]
    public void SimpleReplaceDependentsTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");
        t.AddDependency("a", "e");

        HashSet<string> replacements = new HashSet<string>{ "f", "g", "h" };

        t.ReplaceDependents("a", replacements);
        Assert.AreEqual(3, t.NumDependencies);
        Assert.AreEqual(0, t.NumDependees("a"));
        IEnumerable<string> deps = t.GetDependents("a");
        foreach (string i in deps) {
            Assert.IsTrue((i == "f") || (i == "g") || (i == "h"));
        }
       
    }

    [TestMethod()]
    public void SimpleGraphsAreIndependentTest() {
        DependencyGraph t = new DependencyGraph();
        DependencyGraph u = new DependencyGraph();
        t.AddDependency("a", "b");
        u.AddDependency("b", "c");

        Assert.AreEqual(1, t.NumDependencies);
        Assert.AreEqual(1, u.NumDependencies);


    }



    [TestMethod()]
    public void EmptyDependeesAndDependents() {
        DependencyGraph t = new DependencyGraph();
        Assert.IsFalse(t.HasDependees("A4"));
        Assert.IsFalse(t.HasDependents("A4"));
    }

    [TestMethod()]
    public void EmptyNumDependencies() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.RemoveDependency("a", "b");

        Assert.AreEqual(0, t.NumDependencies);
        Assert.AreEqual(0, t.NumDependees("a"));
    }
    [TestMethod()]
    public void NumDependenciesOnNewGraph() {
        DependencyGraph t = new DependencyGraph();
        Assert.AreEqual(0, t.NumDependencies);
    }
    [TestMethod()]
    public void NumDependeesBadArgument() {
        DependencyGraph t = new DependencyGraph();
        Assert.AreEqual(0, t.NumDependees("a a"));
    }
    [TestMethod()]
    public void NumDependenciesAfterRemoves() {
        DependencyGraph t = new DependencyGraph();
        t.RemoveDependency("a", "b");
        Assert.AreEqual(0, t.NumDependencies);
        t.AddDependency("c", "d");
        Assert.AreEqual(1, t.NumDependencies);
        Assert.AreEqual(0, t.NumDependees("c"));
        Assert.AreEqual(1, t.NumDependees("d"));
    }

    [TestMethod()]
    public void AddSameDependency() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "b");

        Assert.AreEqual(1, t.NumDependencies);
    }

    [TestMethod()]
    public void CreateGraphFromPS2Example() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("A2", "A1");

        t.AddDependency("A3", "A1");
        t.AddDependency("A3", "A2");

        t.AddDependency("A4", "A2");

        Assert.AreEqual(2, t.NumDependees("A1"));
        Assert.AreEqual(2, t.NumDependees("A2"));
        Assert.AreEqual(0, t.NumDependees("A3"));
        Assert.AreEqual(0, t.NumDependees("A4"));

    }
    [TestMethod()]
    public void addSameDependency() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "b");

        Assert.AreEqual(1, t.NumDependencies);
    }

    [TestMethod()]
    public void HasDependentsTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");

        Assert.AreEqual(true, t.HasDependents("a"));
        CollectionAssert.AreEqual(new[] {"b", "c"}, t.GetDependents("a").ToList());

        t.RemoveDependency("a", "b");
        Assert.AreEqual(true, t.HasDependents("a"));
        CollectionAssert.AreEqual(new[] { "c" }, t.GetDependents("a").ToList());

        t.RemoveDependency("a", "c");
        Assert.AreEqual(false, t.HasDependents("a"));
        HashSet<string> empty = new();
        CollectionAssert.AreEqual(empty.ToList(), t.GetDependents("a").ToList());

    }

    [TestMethod()]
    public void NonExistantNodeTest() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        IEnumerator<string> e = t.GetDependees("x").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        IEnumerator<string> f = t.GetDependents("x").GetEnumerator();
        Assert.IsFalse(f.MoveNext());
    }

    [TestMethod()]
    public void sInDictWithEmptySet() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");

        t.ReplaceDependents("a", new HashSet<string>() );

        Assert.IsFalse(t.HasDependents("a"));
    }

    [TestMethod()]
    public void RemoveAndReplace() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");

        t.ReplaceDependents("a", new HashSet<string>());

        Assert.IsFalse(t.HasDependents("a"));

        t.AddDependency("a", "b");
        Assert.IsTrue(t.HasDependents("a"));
    }

    [TestMethod()]
    public void RemovedDependents() {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");

        t.ReplaceDependents("a", new HashSet<string>());

        Assert.IsFalse(t.HasDependents("a"));

        IEnumerator<string> e1 = t.GetDependents("a").GetEnumerator();
        Assert.IsFalse(e1.MoveNext());

        t.AddDependency("a", "b");
        Assert.IsTrue(t.HasDependents("a"));

        e1 = t.GetDependents("a").GetEnumerator();
        Assert.IsTrue(e1.MoveNext());
        Assert.AreEqual("b", e1.Current);
    }



}
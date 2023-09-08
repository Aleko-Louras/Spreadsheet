// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)
//
// DependencyGraph method implementations by Quinn Pritchett
// Updated September 2023
//

namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
/// </summary>
public class DependencyGraph {

    // Stores all dependency pairs as ValueTuples
    private HashSet<ValueTuple<string, string>> dependencies =
        new();

    // Stores a set of dependents for a node
    // Asking for a set of dependents = "who depends on me"
    private Dictionary<string, HashSet<string>> dependents =
        new();

    // Stores a set of dependents for a node
    // Asking for a set of dependees = "who do I depend upon" 
    private Dictionary<string, HashSet<string>> dependees =
        new();

    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph() {

    }


    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies {
        get { return dependencies.Count; }
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    public int NumDependees(string s) {
        if (HasDependees(s)) {
            return GetDependees(s).Count();
        }
        else {
            return 0;
        }
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s) {
        if (dependents.TryGetValue(s, out HashSet<string>? sDependents)) {
            if (sDependents.Count > 0) {
                return true;
            }
            else { return false; } // If s is in dict, but has empty set
        }
        else { return false; } // s is not in dict
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s) {
        if (dependees.TryGetValue(s, out HashSet<string>? sDependees)) {
            if (sDependees.Count > 0) {
                return true;
            }
            else { return false; }// If s is in dict, but has empty set
        }
        else { return false; } // s in not in dict
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s) {
        if (HasDependents(s)) {
            return dependents[s];
        }
        else {
            if (dependents.ContainsKey(s)) {
                return dependents[s]; // s in is dependents dict, but has none
            }
            else {
                return new HashSet<string>(); // s is not in dependents dict
            }
        }
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s) {
        if (HasDependees(s)) {
            return dependees[s];
        }
        else {
            if (dependees.ContainsKey(s)) {
                return dependees[s]; // s in is dependees dict, but has none
            }
            else {
                return new HashSet<string>(); // s not in dependees dict
            }
        }
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t) {
        ValueTuple<string, string> pair = (s, t);

        dependencies.Add(pair);

        // A dependecy was added, which implies
        // we added a dependent and dependee
        AddDependent(s, t);
        AddDependee(s, t);
    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t) {
        ValueTuple<string, string> pair = (s, t);

        if (dependencies.Remove(pair)) {
            dependees[t].Remove(s);
            dependents[s].Remove(t);
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents) {

        List<ValueTuple<string, string>> removedDependencies =
            new();

        foreach ((string, string) pair in dependencies) {
            if (pair.Item1 == s) {
                removedDependencies.Add(pair);
            }
        }

        foreach ((string, string) pair in removedDependencies) {
            RemoveDependency(pair.Item1, pair.Item2);
        }

        foreach (string dependent in newDependents) {
            AddDependency(s, dependent);
        }

    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees) {

        List<ValueTuple<string, string>> removedDependencies =
            new();

        foreach ((string, string) tuple in dependencies) {
            if (tuple.Item2 == s) {
                removedDependencies.Add(tuple);
            }
        }

        foreach (var pair in removedDependencies) {
            RemoveDependency(pair.Item1, pair.Item2);
        }

        foreach (string dependee in newDependees) {
            AddDependency(dependee, s);
        }
    }

    /// <summary>
    /// Adds t to the set of dependents for s
    /// </summary>
    /// <param name="s">The dependee - a node that is depended upon</param>
    /// <param name="t">The dependent - a node that depends on another node</param>
    private void AddDependent(string s, string t) {
        if (!dependents.ContainsKey(s)) {
            dependents.Add(s, new HashSet<string> { t });
        }
        else {
            dependents[s].Add(t); // s already has dependents, add another for t
        }
    }
    /// <summary>
    /// Adds s to the set of dependees for t
    /// </summary>
    /// <param name="s"> The dependee - a node that is depended upon</param>
    /// <param name="t"> The dependent - a node that depends on another node </param>
    private void AddDependee(string s, string t) {
        if (!dependees.ContainsKey(t)) {
            dependees.Add(t, new HashSet<string> { s });
        }
        else {
            dependees[t].Add(s);
        }
    }
}

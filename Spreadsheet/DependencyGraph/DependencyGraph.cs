// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)

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

    private HashSet<ValueTuple<string, string>> dependencies =
        new HashSet<ValueTuple<string, string>>();

    private Dictionary<string, HashSet<string>> dependents =
        new Dictionary<string, HashSet<string>>();

    private Dictionary<string, HashSet<string>> dependees =
        new Dictionary<string, HashSet<string>>();

    private int numDependencies;


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
        if (dependees.TryGetValue(s, out HashSet<string> sDependees)) {
            return sDependees.Count;
        }
        else {
            return 0;
        }
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s) {
        HashSet<string> sDependents;
        if (dependents.TryGetValue(s, out sDependents)) {
            if (sDependents.Count > 0) {
                return true;
            } else { return false; }
        }
        else { return false; }
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s) {
        HashSet<string> sDependees;
        if (dependees.TryGetValue(s, out sDependees)) {
            if (sDependees.Count > 0) {
                return true;
            } else { return false; }
        }
        else { return false; }
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s) {
        if(HasDependents(s)) {
            dependents.TryGetValue(s, out HashSet<string> sDependents);
            return sDependents;
        }
        else {
            if (dependents.ContainsKey(s)) {
                return dependents[s];
            }
            else {
                return new HashSet<string>();
            }
        }
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s) {
        if (HasDependees(s)) {
            dependees.TryGetValue(s, out HashSet<string> sDependees);
            return sDependees;
        }
        else {
            if (dependees.ContainsKey(s)) {
                return dependees[s];
            }
            else {
                return new HashSet<string>();
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

        //If the dependency does not already exist add it
        dependencies.Add(pair);

        // If s is not already in dependents list add it, it now has t as
        // as dependent.
        // If s already has dependents at another for t
        if (!dependents.ContainsKey(s)) {
            dependents.Add(s, new HashSet<string> {t});
        }
        else {
            dependents[s].Add(t);
        }

        // Also must update the dependees, t depends on s
        // if t is not yet in the dependee list, add t with s.
        if(!dependees.ContainsKey(t)) {
            dependees.Add(t, new HashSet<string> {s});
        }
        else {
            dependees[t].Add(s);
        }
            
    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t) {
        ValueTuple<string, string> pair = (s, t);
        dependencies.Remove(pair);

        // t was dependent on s but we removed that link
        // so remove s from t's list of dependees

        dependees[t].Remove(s);

        // Then remove t from s's list of dependents

        dependents[s].Remove(t);

    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents) {
        
        List<ValueTuple<string, string>> removedDependencies =
            new List<ValueTuple<string, string>>();

        foreach ((string,string) tuple in dependencies) {
            if (tuple.Item1 == s) {
                removedDependencies.Add(tuple);
            }
        }

        foreach((string, string) pair in removedDependencies) {
            RemoveDependency(pair.Item1, pair.Item2);
        }

        foreach(string dependent in newDependents) {
            AddDependency(s, dependent);
        }
        
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees) {

        List<ValueTuple<string, string>> removedDependencies =
            new List<ValueTuple<string, string>>();

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

    private void addDependent(string s, string t) {
        if(!dependents.ContainsKey(s)) {
            dependents[s] = new HashSet<string>();
        }
        dependents[s].Add(t);
    }
    private void addDependee(string s, string t) {
        if (!dependees.ContainsKey(s)) {
            dependees[s].Add(s);
        }
        dependees[s].Add(t);
    }
}
using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds.Coloring
{
    public class ClassGraphColorer : BaseAlgorithmObject
    {
        // Helper type for DSATUR
        private class VInfo
        {
            public ExamClass Node { get; }
            public int Color { get; set; } = -1;
            public HashSet<int> Saturation { get; } = new();
            public int Degree { get; }
            public VInfo(ExamClass node, int degree)
            {
                Node = node;
                Degree = degree;
            }
        }

        // Equality comparer for tabu moves using reference equality for StudyClass
        private class MoveComparer : IEqualityComparer<(ExamClass, int)>
        {
            public bool Equals((ExamClass, int) x, (ExamClass, int) y)
            {
                return ReferenceEquals(x.Item1, y.Item1) && x.Item2 == y.Item2;
            }

            public int GetHashCode((ExamClass, int) obj)
            {
                int h1 = RuntimeHelpers.GetHashCode(obj.Item1);
                int h2 = obj.Item2;
                return ((h1 << 5) + h1) ^ h2;
            }
        }

        /// <summary>
        /// Input
        /// </summary>
        public ClassGraph? I_graph { get; set; }
        public Dictionary<ExamClass, HashSet<ExamClass>> I_examClassLinkages { get; set; }
        /// <summary>
        /// Output
        /// </summary>
        public Dictionary<int, HashSet<ExamClass>>? O_color_examClasses { get; set; }
        protected override void ProcedureRun()
        {
            DefaultGraphColoring();
            // var studentYearCount = allStudentYear.GroupBy(x => x.Key).Select(x => (x.Key, x.Count()));

        }

        private void DefaultGraphColoring()
        {
            if (I_graph == null)
                throw new Exception("Graph is null");
            O_color_examClasses = new();
            var sortedVertexByOrder = CreateSortedListHighestOrder(I_graph);
            /*HashSet<Class> nonOrder = sortedVertexByOrder.ToHashSet();*/
            O_color_examClasses = new Dictionary<int, HashSet<ExamClass>>();
            int color = 0;
            HashSet<ExamClass> thisColoredClasses = new();
            while (sortedVertexByOrder.Count > 0)
            {
                sortedVertexByOrder = sortedVertexByOrder.ToList();

                var highestOrderVertex = sortedVertexByOrder[0];
                thisColoredClasses.Add(highestOrderVertex);
                sortedVertexByOrder.RemoveAt(0);
                var locallySortedVertexByOrder = sortedVertexByOrder
                    .ToList();
                while (locallySortedVertexByOrder.Count != 0)
                {
                    var consideringVertex = locallySortedVertexByOrder[0];
                    var edgeVertices = I_graph.AdjacencyList[consideringVertex];

                    // Biến kiểm tra đỉnh có cạnh kề với tập hợp các đỉnh đã được tô màu sẵn hay không
                    bool haveCommonVertex = false;
                    foreach (var edgeVertex in edgeVertices)
                    {
                        if (thisColoredClasses.Contains(edgeVertex))
                        {
                            haveCommonVertex = true;
                            locallySortedVertexByOrder.Remove(consideringVertex);
                            break;
                        }

                    }
                    if (!haveCommonVertex)
                    {
                        thisColoredClasses.Add(consideringVertex);
                        sortedVertexByOrder.Remove(consideringVertex);
                        locallySortedVertexByOrder.Remove(consideringVertex);
                        locallySortedVertexByOrder = locallySortedVertexByOrder.ToList();
                    }
                }
                var dividedCoursesHash = new HashSet<string>();
     
                foreach (var strName in dividedCoursesHash)
                {
                    Console.WriteLine($"Detected name in color : {strName}");
                }
                color++;
                Console.WriteLine($"Color {color}, count class in color {thisColoredClasses.Count}");
                O_color_examClasses.Add(color, thisColoredClasses);
                thisColoredClasses = new();
            }

            foreach (var (colorInd, examClasses) in O_color_examClasses)
            {
                Console.WriteLine($"Color Index {colorInd}");

            }

        }

        public static List<ExamClass> CreateSortedListHighestOrder(ClassGraph classGraph)
        {
            return classGraph.AdjacencyList.Keys.OrderByDescending(x => classGraph.AdjacencyList[x].Count).ToList();
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_graph = context.I_classGraph;
            I_examClassLinkages = context.I_examClass_linkages;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_color_examClasses = O_color_examClasses;
        }

        protected override void InitializeAllOutput()
        {
            O_color_examClasses = new();
        }

        public Dictionary<int, HashSet<ExamClass>> DSATURColoring(bool populateOutput = true)
        {
            if (I_graph == null)
                throw new Exception("Graph is null");


            // Initialize infos
            var infos = I_graph.AdjacencyList.Keys.ToDictionary(
                sc => sc,
                sc => new VInfo(sc, I_graph.AdjacencyList.TryGetValue(sc, out var neigh) ? neigh.Count : 0)
            );

            // Helper to get uncolored infos
            bool HasUncolored() => infos.Values.Any(v => v.Color == -1);

            while (HasUncolored())
            {
                // select vertex with max saturation degree, tie-break by degree
                var candidates = infos.Values.Where(v => v.Color == -1);
                var maxSat = candidates.Max(v => v.Saturation.Count);
                var topBySat = candidates.Where(v => v.Saturation.Count == maxSat);
                var maxDegree = topBySat.Max(v => v.Degree);
                var candidate = topBySat.Where(v => v.Degree == maxDegree).OrderBy(v => v.Node.Count).First();

                // find smallest available color
                int color = 0;
                while (candidate.Saturation.Contains(color)) color++;

                // assign
                candidate.Color = color;

                // update neighbors' saturation sets
                if (I_graph.AdjacencyList.TryGetValue(candidate.Node, out var neighbors))
                {
                    foreach (var nb in neighbors)
                    {
                        var infoNb = infos[nb];
                        if (infoNb.Color == -1)
                        {
                            infoNb.Saturation.Add(color);
                        }
                    }
                }
            }

            // build color groups
            var groups = infos.Values.GroupBy(v => v.Color)
                .ToDictionary(g => g.Key, g => g.Select(v => v.Node).ToHashSet());

            if (populateOutput)
            {
                O_color_examClasses = groups;

            }

            return groups;
        }

        private Dictionary<int, HashSet<ExamClass>> LocalSearchImproveColors(Dictionary<int, HashSet<ExamClass>> initialColors, int maxIterations = 1000, bool populateOutput = true)
        {
            if (I_graph == null)
                throw new Exception("Graph is null");

            // Make a working deep copy of initial coloring
            var colors = initialColors.ToDictionary(kvp => kvp.Key, kvp => new HashSet<ExamClass>(kvp.Value));

            // Normalize color keys to contiguous 0..K-1 to simplify processing
            List<int> NormalizeKeys()
            {
                var sortedKeys = colors.Keys.OrderBy(k => k).ToList();
                var mapping = new Dictionary<int, int>();
                for (int i = 0; i < sortedKeys.Count; i++) mapping[sortedKeys[i]] = i;
                var newColors = new Dictionary<int, HashSet<ExamClass>>();
                foreach (var kv in colors)
                {
                    newColors[mapping[kv.Key]] = new HashSet<ExamClass>(kv.Value);
                }
                colors = newColors;
                return colors.Keys.OrderBy(k => k).ToList();
            }

            NormalizeKeys();

            int iter = 0;
            bool changed = true;
            while (changed && iter < maxIterations)
            {
                iter++;
                changed = false;

                // Try to move vertices from highest colors into lower colors
                int maxColor = colors.Keys.Max();
                // Iterate colors from highest down to 1 (we won't try to move into higher colors)
                for (int c = maxColor; c >= 1 && !changed; c--)
                {
                    if (!colors.TryGetValue(c, out var bucket) || bucket.Count == 0)
                        continue;

                    // iterate over a snapshot of vertices in this color
                    foreach (var v in bucket.ToList())
                    {
                        // find a target color t < c where v has no neighbor in that color
                        for (int t = 0; t < c; t++)
                        {
                            bool conflict = false;
                            if (I_graph.AdjacencyList.TryGetValue(v, out var neighbors))
                            {
                                foreach (var nb in neighbors)
                                {
                                    if (colors.TryGetValue(t, out var setT) && setT.Contains(nb))
                                    {
                                        conflict = true;
                                        break;
                                    }
                                }
                            }

                            if (!conflict)
                            {
                                // move v from c to t
                                colors[c].Remove(v);
                                if (!colors.ContainsKey(t)) colors[t] = new HashSet<ExamClass>();
                                colors[t].Add(v);
                                changed = true;
                                break; // moved this vertex, go to next vertex
                            }
                        }

                        if (changed) break; // restart outer loops after a successful move
                    }

                    // if this color became empty remove it and renormalize
                    if (colors.TryGetValue(c, out var maybeEmpty) && maybeEmpty.Count == 0)
                    {
                        colors.Remove(c);
                        NormalizeKeys();
                        changed = true;
                        break; // restart main loop
                    }
                }

                // If no improvement by moving from top colors, try greedy recolor: scan all vertices and assign smallest feasible color
                if (!changed)
                {
                    // Build list of all vertices
                    var allVertices = colors.Values.SelectMany(s => s).ToList();
                    // Clear colors
                    colors = new Dictionary<int, HashSet<ExamClass>>();

                    foreach (var v in allVertices)
                    {
                        // compute forbidden colors for v
                        var forbidden = new HashSet<int>();
                        if (I_graph.AdjacencyList.TryGetValue(v, out var neighs))
                        {
                            foreach (var nb in neighs)
                            {
                                // find color of neighbor
                                foreach (var (col, set) in colors)
                                {
                                    if (set.Contains(nb))
                                    {
                                        forbidden.Add(col);
                                        break;
                                    }
                                }
                            }
                        }

                        // find smallest color not forbidden
                        int assign = 0;
                        while (forbidden.Contains(assign)) assign++;
                        if (!colors.ContainsKey(assign)) colors[assign] = new HashSet<ExamClass>();
                        colors[assign].Add(v);
                    }

                    NormalizeKeys();

                    // if recoloring reduced number of colors mark changed=true to allow further squeezing
                    // (we compare previous count via allVertices grouping)
                    // changed = true if recolor produced fewer colors than before
                    // we'll detect reduction implicitly next iteration
                }
            }

            // optionally populate outputs
            if (populateOutput)
            {
                O_color_examClasses = colors;

            }

            return colors;
        }

        private Dictionary<int, HashSet<ExamClass>> LocalSearchSimulatedAnnealingColors(
            Dictionary<int, HashSet<ExamClass>> initialColors,
            double initialTemperature = 10.0,
            double coolingRate = 0.95,
            int iterationsPerTemp = 200,
            double minTemperature = 1e-3,
            int maxIterations = 10000,
            int? randomSeed = null,
            bool populateOutput = true)
        {
            if (I_graph == null)
                throw new Exception("Graph is null");

            var rng = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

            // Work on a mutable copy
            var colors = initialColors.ToDictionary(kvp => kvp.Key, kvp => new HashSet<ExamClass>(kvp.Value));

            // Helper: normalize color keys to contiguous 0..K-1
            void NormalizeKeys()
            {
                var sortedKeys = colors.Keys.OrderBy(k => k).ToList();
                var mapping = new Dictionary<int, int>();
                for (int i = 0; i < sortedKeys.Count; i++) mapping[sortedKeys[i]] = i;
                var newColors = new Dictionary<int, HashSet<ExamClass>>();
                foreach (var kv in colors)
                {
                    newColors[mapping[kv.Key]] = new HashSet<ExamClass>(kv.Value);
                }
                colors = newColors;
            }

            NormalizeKeys();

            // Build mapping StudyClass -> color for quick lookup
            Dictionary<ExamClass, int> BuildMapping(Dictionary<int, HashSet<ExamClass>> cols)
            {
                var map = new Dictionary<ExamClass, int>();
                foreach (var kv in cols)
                {
                    foreach (var sc in kv.Value)
                        map[sc] = kv.Key;
                }
                return map;
            }

            Dictionary<ExamClass, int> currentMap = BuildMapping(colors);

            // Cost: weight conflicts highly so SA tries to eliminate them first, then minimizes number of colors
            int CountConflicts(Dictionary<ExamClass, int> map)
            {
                int conflicts = 0;
                var seen = new HashSet<(ExamClass, ExamClass)>();
                foreach (var kv in I_graph.AdjacencyList)
                {
                    var v = kv.Key;
                    if (!map.TryGetValue(v, out var c1)) continue;
                    foreach (var nb in kv.Value)
                    {
                        if (!map.TryGetValue(nb, out var c2)) continue;
                        if (c1 == c2)
                        {
                            // count each unordered pair once
                            var pair = (v, nb);
                            var pairRev = (nb, v);
                            if (!seen.Contains(pair) && !seen.Contains(pairRev))
            {
                                seen.Add(pair);
                                conflicts++;
                            }
                        }
                    }
                }
                return conflicts;
            }

            int CountColors(Dictionary<int, HashSet<ExamClass>> cols) => cols.Keys.Count;

            double CostForMap(Dictionary<ExamClass, int> map, Dictionary<int, HashSet<ExamClass>> cols)
            {
                // conflict penalty coefficient (large)
                const double conflictCoeff = 1000.0;
                int conflicts = CountConflicts(map);
                int ncolors = CountColors(cols);
                return conflicts * conflictCoeff + ncolors;
            }

            double T = initialTemperature;
            var currentColors = colors.ToDictionary(k => k.Key, k => new HashSet<ExamClass>(k.Value));
            var currentMapping = BuildMapping(currentColors);
            double currentCost = CostForMap(currentMapping, currentColors);

            // best found
            var bestColors = currentColors.ToDictionary(k => k.Key, k => new HashSet<ExamClass>(k.Value));
            var bestMapping = BuildMapping(bestColors);
            double bestCost = currentCost;

            int iterations = 0;

            while (T > minTemperature && iterations < maxIterations)
            {
                for (int it = 0; it < iterationsPerTemp; it++)
                {
                    iterations++;
                    // propose a random move: pick a random vertex and random target color (including new color)
                    var allVertices = currentMapping.Keys.ToList();
                    if (allVertices.Count == 0) break;
                    var v = allVertices[rng.Next(allVertices.Count)];
                    int currentColor = currentMapping[v];
                    int maxColorIndex = currentColors.Keys.Max();
                    int targetColor = rng.Next(maxColorIndex + 2); // [0 .. maxColorIndex+1], allow new color

                    // make a tentative copy of structures for the move
                    var newColors = currentColors.ToDictionary(k => k.Key, k => new HashSet<ExamClass>(k.Value));
                    var newMapping = new Dictionary<ExamClass, int>(currentMapping);

                    // ensure target color exists
                    if (!newColors.ContainsKey(targetColor)) newColors[targetColor] = new HashSet<ExamClass>();
                    // perform move
                    newColors[currentColor].Remove(v);
                    newColors[targetColor].Add(v);
                    newMapping[v] = targetColor;

                    // if current color became empty, remove it and remap keys to keep compact; update mapping accordingly
                    if (newColors[currentColor].Count == 0)
                    {
                        // remove and renumber
                        var ordered = newColors.Keys.OrderBy(k => k).ToList();
                        var mapping = new Dictionary<int, int>();
                        for (int i = 0; i < ordered.Count; i++) mapping[ordered[i]] = i;
                        var compact = new Dictionary<int, HashSet<ExamClass>>();
                        foreach (var kv in newColors)
                        {
                            compact[mapping[kv.Key]] = new HashSet<ExamClass>(kv.Value);
                        }
                        newColors = compact;
                        // rebuild newMapping
                        newMapping = BuildMapping(newColors);
                    }

                    double newCost = CostForMap(newMapping, newColors);
                    double delta = newCost - currentCost;
                    bool accept = false;
                    if (delta <= 0) accept = true;
                    else
                    {
                        double p = Math.Exp(-delta / T);
                        if (rng.NextDouble() < p) accept = true;
                    }

                    if (accept)
                    {
                        currentColors = newColors;
                        currentMapping = newMapping;
                        currentCost = newCost;

                        if (currentCost < bestCost)
                        {
                            bestCost = currentCost;
                            bestColors = currentColors.ToDictionary(k => k.Key, k => new HashSet<ExamClass>(k.Value));
                            bestMapping = BuildMapping(bestColors);
                        }
                    }

                    // early termination if perfect (0 conflicts and minimal possible colors maybe)
                    if (bestCost < 1.0) // zero conflicts and 0 or 1 color
                        break;
                }

                T *= coolingRate;
            }

            // final normalization of bestColors keys
            // compact indices to 0..K-1
            var finalOrdered = bestColors.Keys.OrderBy(k => k).ToList();
            var finalMapping = new Dictionary<int, int>();
            for (int i = 0; i < finalOrdered.Count; i++) finalMapping[finalOrdered[i]] = i;
            var finalColors = new Dictionary<int, HashSet<ExamClass>>();
            foreach (var kv in bestColors)
            {
                finalColors[finalMapping[kv.Key]] = new HashSet<ExamClass>(kv.Value);
            }

            if (populateOutput)
            {
                O_color_examClasses = finalColors;
            }

            return finalColors;
        }

        public Dictionary<int, HashSet<ExamClass>> LocalSearchTabuColors(
            Dictionary<int, HashSet<ExamClass>> initialColors,
            int tabuTenure = 7,
            int maxIterations = 1000,
            int neighborhoodSampleSize = 0,
            int? randomSeed = null,
            bool populateOutput = true)
        {
            if (I_graph == null)
                throw new Exception("Graph is null");

            var rng = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

            // Working copy of colors
            var colors = initialColors.ToDictionary(entry => entry.Key, entry => new HashSet<ExamClass>(entry.Value));

            void NormalizeKeysLocal()
            {
                var ordered = colors.Keys.OrderBy(k => k).ToList();
                var map = new Dictionary<int, int>();
                for (int i = 0; i < ordered.Count; i++) map[ordered[i]] = i;
                var newColors = new Dictionary<int, HashSet<ExamClass>>();
                foreach (var kv in colors)
                    newColors[map[kv.Key]] = new HashSet<ExamClass>(kv.Value);
                colors = newColors;
            }

            NormalizeKeysLocal();

            Dictionary<ExamClass, int> BuildMappingLocal(Dictionary<int, HashSet<ExamClass>> cols)
            {
                var map = new Dictionary<ExamClass, int>();
                foreach (var kv in cols)
                {
                    foreach (var sc in kv.Value)
                        map[sc] = kv.Key;
                }
                return map;
            }

            var currentColors = colors.ToDictionary(e => e.Key, e => new HashSet<ExamClass>(e.Value));
            var currentMap = BuildMappingLocal(currentColors);

            int CountConflictsLocal(Dictionary<ExamClass, int> map)
            {
                int conflicts = 0;
                var seen = new HashSet<(ExamClass, ExamClass)>();
                foreach (var kv in I_graph.AdjacencyList)
                {
                    var v = kv.Key;
                    if (!map.TryGetValue(v, out var c1)) continue;
                    foreach (var nb in kv.Value)
                    {
                        if (!map.TryGetValue(nb, out var c2)) continue;
                        if (c1 == c2)
                        {
                            var pair = (v, nb);
                            var rev = (nb, v);
                            if (!seen.Contains(pair) && !seen.Contains(rev))
                            {
                                seen.Add(pair);
                                conflicts++;
                            }
                        }
                    }
                }
                return conflicts;
            }

            int CountColorsLocal(Dictionary<int, HashSet<ExamClass>> cols) => cols.Keys.Count;

            double CostForMapLocal(Dictionary<ExamClass, int> map, Dictionary<int, HashSet<ExamClass>> cols)
            {
                const double conflictCoeff = 1000.0;
                int conflicts = CountConflictsLocal(map);
                int ncolors = CountColorsLocal(cols);
                return conflicts * conflictCoeff + ncolors;
            }

            double currentCost = CostForMapLocal(currentMap, currentColors);
            var bestColors = currentColors.ToDictionary(e => e.Key, e => new HashSet<ExamClass>(e.Value));
            var bestMap = BuildMappingLocal(bestColors);
            double bestCost = currentCost;

            var tabu = new Dictionary<(ExamClass, int), int>(new MoveComparer());
            int iter = 0;
            var verticesAll = currentMap.Keys.ToList();
            if (neighborhoodSampleSize <= 0) neighborhoodSampleSize = Math.Max(1, verticesAll.Count);

            while (iter < maxIterations)
            {
                iter++;
                // sample vertices
                var sample = currentMap.Keys.OrderBy(_ => rng.Next()).Take(neighborhoodSampleSize).ToList();
                double bestCandidateCost = double.PositiveInfinity;
                (ExamClass v, int from, int to) bestMove = (null!, -1, -1);

                int maxColorIndex = currentColors.Count == 0 ? -1 : currentColors.Keys.Max();

                foreach (var v in sample)
                {
                    int from = currentMap[v];
                    for (int to = 0; to <= maxColorIndex + 1; to++)
                    {
                        if (to == from) continue;
                        // check tabu
                        bool isTabu = tabu.TryGetValue((v, to), out var expiry) && expiry > iter;

                        // create tentative colors
                        var trialColors = currentColors.ToDictionary(e => e.Key, e => new HashSet<ExamClass>(e.Value));
                        if (!trialColors.ContainsKey(to)) trialColors[to] = new HashSet<ExamClass>();
                        trialColors[from].Remove(v);
                        trialColors[to].Add(v);
                        if (trialColors[from].Count == 0) trialColors.Remove(from);

                        var trialMap = BuildMappingLocal(trialColors);
                        double trialCost = CostForMapLocal(trialMap, trialColors);

                        if (isTabu && trialCost >= bestCost) continue; // aspiration

                        if (trialCost < bestCandidateCost)
                        {
                            bestCandidateCost = trialCost;
                            bestMove = (v, from, to);
                        }
                    }
                }

                if (bestCandidateCost == double.PositiveInfinity)
                    break;

                // apply best move
                var move = bestMove;
                if (!currentColors.ContainsKey(move.to)) currentColors[move.to] = new HashSet<ExamClass>();
                currentColors[move.from].Remove(move.v);
                currentColors[move.to].Add(move.v);
                currentMap[move.v] = move.to;
                if (currentColors.ContainsKey(move.from) && currentColors[move.from].Count == 0)
                    currentColors.Remove(move.from);

                currentCost = bestCandidateCost;

                // add tabu for reverse move
                tabu[(move.v, move.from)] = iter + tabuTenure;

                if (currentCost < bestCost)
                {
                    bestCost = currentCost;
                    bestColors = currentColors.ToDictionary(e => e.Key, e => new HashSet<ExamClass>(e.Value));
                    bestMap = BuildMappingLocal(bestColors);
                }
            }

            // compact final keys
            var finalOrdered = bestColors.Keys.OrderBy(k => k).ToList();
            var finalMap = new Dictionary<int, int>();
            for (int i = 0; i < finalOrdered.Count; i++) finalMap[finalOrdered[i]] = i;
            var finalColors = new Dictionary<int, HashSet<ExamClass>>();
            foreach (var kv in bestColors)
                finalColors[finalMap[kv.Key]] = new HashSet<ExamClass>(kv.Value);

            if (populateOutput)
            {
                O_color_examClasses = finalColors;
            }

            return finalColors;
        }
    }
}

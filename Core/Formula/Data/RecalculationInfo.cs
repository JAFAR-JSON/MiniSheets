using MiniSheets.Core.Model;

namespace MiniSheets.Core.Formula;

/// <summary>
/// SorterEvaluationOrder is sorted from first to evaluate to last.
/// </summary>
public record struct RecalculationInfo
    (List<CellKey> SortedEvaluationOrder, List<CellKey> Circular);

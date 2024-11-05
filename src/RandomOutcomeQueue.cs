namespace MyGameName;

using System;
using System.Collections.Generic;
using System.Linq;
using CLSS;
using Godot;

public sealed class RandomOutcomeQueue<TOutcome> where TOutcome : IEquatable<TOutcome> {
  public struct OutcomeParams {
    public int Weight { get; set; }
    public int MaxTriesBeforeForced { get; set; }
  }
  private struct OutcomeSelectionData {
    public int TurnsSinceLastSelected { get; set; }
    public bool InForcedSelectionQueue { get; set; }
  }
  private Dictionary<TOutcome, OutcomeSelectionData> _outcomeSelectionData = new();
  private Dictionary<TOutcome, OutcomeParams> _outcomeParams = new();
  private Queue<TOutcome> _forcedSelectionQueue = new();
  private int _maxWeight = -1;

  public RandomOutcomeQueue() {
  }

  public bool TryGetOutcomeParams(TOutcome outcome, out OutcomeParams outcomeParams) {
    var result = _outcomeParams.TryGetValue(outcome, out outcomeParams);
    if (result) {
      return result;
    }
    outcomeParams = default!;
    return false;
  }

  public void AddOutcome(TOutcome outcome, OutcomeParams outcomeParams) {
    _outcomeParams.Add(outcome, outcomeParams);
    _outcomeSelectionData.Add(outcome, new());
  }

  public void RemoveOutcome(TOutcome outcome) {
    _outcomeParams.Remove(outcome);
    _outcomeSelectionData.Remove(outcome);
  }

  public TOutcome GetNext() {
    TOutcome selection;

    var outcomes = _outcomeSelectionData.Keys;

    if (_forcedSelectionQueue.Count > 0) {
      selection = _forcedSelectionQueue.Dequeue()!;
      _outcomeSelectionData[selection] = _outcomeSelectionData[selection] with { InForcedSelectionQueue = false };
    }
    else {
      selection = WeightedRandomPick();
    }

    outcomes.ForEach((TOutcome outcome) => UpdateSelectionData(outcome, ref selection));
    outcomes.ForEach((TOutcome key) => QueueOutcomeIfNeeded(key, ref selection));
    return selection;
  }

  private void UpdateSelectionData(TOutcome outcome, ref TOutcome selection) {
    var selectionData = _outcomeSelectionData[outcome];
    if (selection.Equals(outcome)) {
      _outcomeSelectionData[outcome] = selectionData with { TurnsSinceLastSelected = 0 };
    }
    else {
      _outcomeSelectionData[outcome] = selectionData with { TurnsSinceLastSelected = selectionData.TurnsSinceLastSelected + 1 };
    }
  }

  private void QueueOutcomeIfNeeded(TOutcome outcome, ref TOutcome selection) {
    if (selection.Equals(outcome)) {
      return;
    }
    var selectionData = _outcomeSelectionData[outcome];
    if (selectionData.InForcedSelectionQueue || selectionData.TurnsSinceLastSelected < (_outcomeParams[outcome].MaxTriesBeforeForced - 1)) {
      return;
    }
    _forcedSelectionQueue.Enqueue(outcome);
    _outcomeSelectionData[outcome] = selectionData with { InForcedSelectionQueue = true };
  }

  private TOutcome WeightedRandomPick(int maxPickTries = 10) {
    // Weighted random selection
    var weightSum = _outcomeParams.Sum((outcome) => outcome.Value.Weight);
    for (int i = 0; i < maxPickTries; i++) {
      var pick = GD.Randi() % weightSum;
      var weightMinBound = 0;
      foreach (var outcome in _outcomeParams.Keys) {
        var weight = _outcomeParams[outcome].Weight;
        if (pick >= weightMinBound && pick < (weightMinBound + weight)) {
          return outcome;
        }
        weightMinBound += weight;
      }
    }
    Logger.LogWarning($"{nameof(RandomOutcomeQueue<TOutcome>)}: Was not able to select an item in {maxPickTries} tries, resorting to fully random pick.");
    return _outcomeParams.Keys.ToList()[(int)GD.Randi() % _outcomeParams.Values.Count];
  }
}

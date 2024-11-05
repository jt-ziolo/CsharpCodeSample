namespace MyGameName;

using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using GodotTestDriver;
using Shouldly;
using Chickensoft.GoDotLog;
using System.Collections.Generic;

public class RandomOutcomeQueueTest : TestClass {
  private readonly ILog _log = new GDLog(nameof(SaveAndLoadTest));
  private Fixture _fixture = default!;

  public RandomOutcomeQueueTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
  }

  [CleanupAll]
  public void Cleanup() => _fixture.Cleanup();

  [Test]
  public void TestLooseCounts() {
    var random = new RandomOutcomeQueue<char>();
    random.AddOutcome('A', new RandomOutcomeQueue<char>.OutcomeParams { Weight = 100, MaxTriesBeforeForced = int.MaxValue });
    random.AddOutcome('B', new RandomOutcomeQueue<char>.OutcomeParams { Weight = 100, MaxTriesBeforeForced = int.MaxValue });
    random.AddOutcome('C', new RandomOutcomeQueue<char>.OutcomeParams { Weight = 1, MaxTriesBeforeForced = int.MaxValue });
    Dictionary<char, int> counter = new();
    for (int i = 0; i < 1000; i++) {
      var next = random.GetNext();
      counter.SetBasedOnCurrentValue(next, curr => curr + 1, 1);
    }
    counter['A'].ShouldBeInRange(470, 530);
    counter['B'].ShouldBeInRange(470, 530);
    counter['C'].ShouldBeInRange(0, 20);
  }

  [Test]
  public void TestMaxTriesBeforeForced() {
    var random = new RandomOutcomeQueue<char>();
    random.AddOutcome('A', new RandomOutcomeQueue<char>.OutcomeParams { Weight = 1, MaxTriesBeforeForced = 100 });
    random.AddOutcome('B', new RandomOutcomeQueue<char>.OutcomeParams { Weight = 1000000000, MaxTriesBeforeForced = 100 });
    Dictionary<char, int> counter = new();
    for (int i = 0; i < 1000; i++) {
      var next = random.GetNext();
      counter.SetBasedOnCurrentValue(next, curr => curr + 1, 1);
    }
    counter['A'].ShouldBeInRange(10, 11);
  }
}

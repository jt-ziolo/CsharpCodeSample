namespace MyGameName;

using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using GodotTestDriver;
using Shouldly;
using Chickensoft.GoDotLog;


public class SaveAndLoadTest : TestClass {
  private readonly ILog _log = new GDLog(nameof(SaveAndLoadTest));
  private Fixture _fixture = default!;

  public SaveAndLoadTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
  }

  [CleanupAll]
  public void Cleanup() => _fixture.Cleanup();

  [Test]
  public void TestHelloWorld() {
    var hello = "Hello, World!";
    hello.ShouldBe("Hello, World!");
  }
}

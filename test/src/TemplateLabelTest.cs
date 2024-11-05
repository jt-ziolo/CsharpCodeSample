namespace MyGameName;

using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using GodotTestDriver;
using Shouldly;
using Chickensoft.GoDotLog;
using Chickensoft.AutoInject;
using System;
using System.Collections.Generic;
using System.Linq;


public class TemplateLabelTest : TestClass {
  private Node _scene = default!;
  private TemplateLabelLookupService _lookup = default!;

  private readonly ILog _log = new GDLog(nameof(TemplateLabelTest));
  private Fixture _fixture = default!;

  public TemplateLabelTest(Node testScene) : base(testScene) { }

  [Setup]
  public async Task SetupAll() {
    _fixture = new Fixture(TestScene.GetTree());
    _scene = await _fixture.LoadScene<Node>("test/scenes/TemplateLabelTest.tscn");
    _lookup = _fixture.AutoFree(new TemplateLabelLookupService());
    _scene.AddChild(_lookup);
  }

  // [Cleanup]
  // public async Task Cleanup() {
  //   _lookup.ScriptObject.Clear();
  // }

  [Cleanup]
  public void CleanupAll() => _fixture.Cleanup();

  private void PreSetupAssertsAndSetup(TemplateLabel label, string initialTextContent) {
    _lookup.ShouldNotBeNull();
    label.ShouldNotBeNull();
    label.Text.ShouldBe(initialTextContent);
    label.FakeDependency(_lookup);
    label._Notification((int)Node.NotificationReady);
    label.Lookup.ShouldBe(_lookup);
  }

  [Test]
  public async Task TestHelloWorldAsync() {
    var helloWorld = (TemplateLabel)_scene.GetNode("%HelloWorld")!;
    PreSetupAssertsAndSetup(helloWorld, "Hello {{ what }}!");

    helloWorld.OnResolved();
    // don't add anything to the dictionary yet, and test
    _lookup.ScriptObject.Count.ShouldBe(0);

    PreSetupAssertsAndSetup(helloWorld, "Hello {{ what }}!");

    // add entries to the dictionary and test
    _lookup.ScriptObject.Add("what", "World");
    await Task.Delay(TimeSpan.FromMilliseconds(100));

    _lookup.ScriptObject["what"].ShouldBe("World");

    helloWorld._Process(0.1);
    helloWorld.Text.ShouldBe("Hello World!");
  }

  [Test]
  public async Task TestPluralizeAsync() {
    var pluralize = (TemplateLabel)_scene.GetNode("%Pluralize")!;
    PreSetupAssertsAndSetup(pluralize, "There are {{ products.size }} {{ products.size | string.pluralize 'product' 'products' }}.");

    pluralize.OnResolved();
    // don't add anything to the dictionary yet, and test
    _lookup.ScriptObject.Count.ShouldBe(0);

    PreSetupAssertsAndSetup(pluralize, "There are {{ products.size }} {{ products.size | string.pluralize 'product' 'products' }}.");

    // add entries to the dictionary and test
    List<string> products = ["apples", "bananas", "oranges"];
    _lookup.ScriptObject.Add("products", products);
    await Task.Delay(TimeSpan.FromMilliseconds(100));

    _lookup.ScriptObject["products"].ShouldBe(products);

    pluralize._Process(0.1);
    pluralize.Text.ShouldBe("There are 3 products.");
  }

  [Test]
  public async Task TestMathFormatAsync() {
    var mathFormat = (TemplateLabel)_scene.GetNode("%MathFormat")!;
    PreSetupAssertsAndSetup(mathFormat, "{{ longDecimal | math.format 'F3' }}");

    mathFormat.OnResolved();
    // don't add anything to the dictionary yet, and test
    _lookup.ScriptObject.Count.ShouldBe(0);

    PreSetupAssertsAndSetup(mathFormat, "{{ longDecimal | math.format 'F3' }}");

    // add entries to the dictionary and test
    _lookup.ScriptObject.Add("longDecimal", 13.0f / 9.0f);
    await Task.Delay(TimeSpan.FromMilliseconds(100));

    _lookup.ScriptObject["longDecimal"].ShouldBe(13.0f / 9.0f);

    mathFormat._Process(0.1);
    mathFormat.Text.ShouldBe("1.444");
  }
}

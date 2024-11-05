Make sure you set up the game scene correctly if you need to mock something during integration testing!
```cs
public class ExampleTest : TestClass {
  private Game _game = default!;
  
  // ...

  [SetupAll]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _game = await _fixture.LoadAndAddScene<Game>(Main.GAME_SCENE_PATH);
    (_game as IAutoInit).IsTesting = true; // here
  }
}
```

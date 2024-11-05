namespace MyGameName;

using MyGameName.ValueObjects.Names;

public partial class GameSession {
  public GameSession(WorldName worldName, CharacterName characterName) {
    WorldName = worldName;
    CharacterName = characterName;
  }

  public WorldName WorldName { get; private set; } = (WorldName)"Default World";
  public CharacterName CharacterName { get; private set; } = (CharacterName)"Default Character";
}

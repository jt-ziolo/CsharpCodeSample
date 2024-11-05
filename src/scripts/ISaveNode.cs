namespace MyGameName;
using Godot;

public interface ISaveNode {
  public void AddDataToDictionary(Godot.Collections.Dictionary<string, Variant> dictionary);
  public string GetSaveName();
  public void LoadDataFromDictionary(Godot.Collections.Dictionary<string, Variant> dictionary);
  public void OnPostLoad();
  public void OnPostSave();
  public void OnPreLoad();
  public void OnPreSave();
}

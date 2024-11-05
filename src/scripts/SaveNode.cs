namespace MyGameName;
using Godot;
using MyGameName.Enums;

[GlobalClass]
public partial class SaveNode : Node, ISaveNode {
  [Export(PropertyHint.PlaceholderText, "NodeXYZ")]
  public string SaveName { get; set; }

  [Export]
  public Node3D TargetNode { get; set; }

  #region methods

  public override void _Ready() {
    this.AddToGroup(GodotGroup.Persist);
  }

  public void AddDataToDictionary(Godot.Collections.Dictionary<string, Variant> dictionary) {
    TargetNode.SaveBasicData(dictionary);
    TargetNode.SaveNode3D(dictionary);
  }

  public string GetSaveName() => SaveName;

  public void LoadDataFromDictionary(Godot.Collections.Dictionary<string, Variant> dictionary) => TargetNode.LoadNode3D(dictionary);

  public void OnPostLoad() { }

  public void OnPostSave() { }

  public void OnPreLoad() { }

  public void OnPreSave() { }
  #endregion
}

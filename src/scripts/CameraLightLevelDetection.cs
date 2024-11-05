namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class CameraLightLevelDetection : SubViewport, ISaveNode {
  public event EventHandler<float> OnLightLevelChanged;

  #region properties

  [Export(PropertyHint.Range, "0.0,1.0")]
  public float LightLevelSmoothingFactor { get; set; } = 0.25f;

  [Dependency]
  public GameWorld WorldProvider => this.DependOn<GameWorld>();

  private Camera3D FloorProbe => this.GetFirstChildOfType<Camera3D>();
  private int? Height { get; set; }
  private int? Width { get; set; }
  #endregion

  private float _avgLuminance;
  private Rid _textureRid;

  #region methods
  public override void _Notification(int what) => this.Notify(what);

  public void AddDataToDictionary(Godot.Collections.Dictionary<string, Variant> dictionary) {
    this.SaveBasicData(dictionary);
    dictionary.TryAdd("AvgLuminance", _avgLuminance);
  }

  public string GetSaveName() => Name;

  public void LoadDataFromDictionary(Godot.Collections.Dictionary<string, Variant> dictionary) {
    _avgLuminance = (float)dictionary.GetSavedValue("AvgLuminance", 0.0f);
  }

  public void OnFramePostDraw() {
    var image = RenderingServer.Texture2DGet(_textureRid);
    var totalLuminance = 0.0f;
    Width ??= image.GetWidth();
    Height ??= image.GetHeight();
    var numPixels = Width * Height;
    for (int i = 0; i < Width; i++) {
      for (int j = 0; j < Height; j++) {
        var pixel = image.GetPixel(i, j);
        totalLuminance += pixel.Luminance;
      }
    }
    var newAvg = totalLuminance / numPixels;
    _avgLuminance = Mathf.Lerp(_avgLuminance, newAvg.Value, LightLevelSmoothingFactor);
    OnLightLevelChanged?.Invoke(this, _avgLuminance);
    // Logger.Log(_avgLuminance.ToString());
    // Move to the new player position prior to the next viewport draw call
    FloorProbe.GlobalPosition = WorldProvider.Player.GlobalPosition;
  }

  public void OnPostLoad() {
    OnLightLevelChanged?.Invoke(this, _avgLuminance);
  }

  public void OnPostSave() { }

  public void OnPreLoad() { }

  public void OnPreSave() { }

  public void Initialize() {
    _textureRid = GetTexture().GetRid();
  }

  public void OnResolved() {
    RenderingServer.FramePostDraw += OnFramePostDraw;
  }

  #endregion
}

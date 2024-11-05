namespace MyGameName;
using CLSS;
using Godot;
using MyGameName.ValueObjects;
using MyGameName.ValueObjects.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ModLoadingService : Node {
  internal static readonly List<ModData> _modData = new();

  #region methods

  internal void DisableAllMods() => _modData.ForEach(mod => {
    mod.ModSceneRootNode.RemoveFromGroup(Enums.GodotGroup.ModPersist);
    mod.ModSceneRootNode.CallThreadSafe("DisableMod");
  });

  internal void DisableMod(params ModName[] modNames) {
    var modNameSet = modNames.ToHashSet();
    _modData.Where(mod => modNameSet.Contains(mod.Name))
      .ForEach(mod => {
        mod.ModSceneRootNode.RemoveFromGroup(Enums.GodotGroup.ModPersist);
        mod.ModSceneRootNode.CallThreadSafe("DisableMod");
      });
  }

  internal void LoadModsStartup() {
    LoadMods();
    StartMods();
  }

  internal void StartMods() => _modData.ForEach(mod => {
    mod.ModSceneRootNode.AddToGroup(Enums.GodotGroup.ModPersist);
    mod.ModSceneRootNode.CallThreadSafe("EnableMod");
  });

  private static IEnumerable<(FileName, FileExtension)> GetModPackageNamesAndExtensions() {
    var modsDir = PathUtility.GetGlobalizedPathSafe((GodotPath)"res://mods/");
    var files = PathUtility.GetFileNamesAndExtensionsInSubDirectories(modsDir);
    return files.Where(tuple => tuple.Item2.Value is ".pck" or ".zip");
  }

  private void LoadMods() {
    var modNames = GetModPackageNamesAndExtensions();
    var modsLoadedCount = 0;
    foreach (var (name, ext) in modNames) {
      // var assemblyPath = PathUtility.GetGlobalizedPathSafe((GodotPath)$"res://mods/{name}/{name}.dll");
      // Assembly assembly;
      // Version version;
      // try {
      // assembly = Assembly.LoadFile(assemblyPath.Value);
      // }
      // catch (Exception error) {
      // Logger.LogWarning(ToString(), $"Mod assembly at path ({assemblyPath}) did not load successfully: {error.Message}. Skipping...");
      // continue;
      // }
      // try {
      // version = (Version)assembly.GetName().Version!.Clone();
      // }
      // catch (Exception error) {
      // Logger.LogWarning(ToString(), $"Mod assembly at path ({assemblyPath}) lacks a version: {error.Message}. Skipping...");
      // continue;
      // }
      var packagePath = PathUtility.GetGlobalizedPathSafe((GodotPath)$"res://mods/{name}/{name}.{ext}");
      var wasSuccessful = ProjectSettings.LoadResourcePack(packagePath.Value);
      if (!wasSuccessful) {
        Logger.LogWarning(ToString(), $"Mod at path ({packagePath}) did not load successfully. Skipping...");
        continue;
      }
      modsLoadedCount++;
      var scene = (PackedScene)ResourceLoader.Load(PathUtility.GetGlobalizedPathSafe((GodotPath)$"res://{name}.tscn").Value);
      var sceneNode = scene.Instantiate(PackedScene.GenEditState.Instance);
      var modDataEntry = new ModData(
        sceneNode.GetMeta("_authors").As<string[]>(),
        ModName.From(sceneNode.GetMeta("_name").As<string>()),
        sceneNode.GetMeta("_loadBefore").As<string[]>(),
        sceneNode.GetMeta("_loadAfter").As<string[]>(),
        sceneNode.GetMeta("_incompatibleWith").As<string[]>(),
        Version.Parse(sceneNode.GetMeta("_version").As<string>()),
        sceneNode.GetMeta("_icon").As<Texture2D>(),
        sceneNode.GetMeta("_config").As<ConfigFile>(),
        scene,
        sceneNode
      );
      _modData.Add(modDataEntry);
      Logger.Log(this, Logger.LogColorByPurpose.Success, $"Loaded mod: {modDataEntry}");
    }
    Logger.Log(this, Logger.LogColorByPurpose.Success, $"Loading finished, found: {modNames.Count()}, loaded: {modsLoadedCount}");
  }

  #endregion
}

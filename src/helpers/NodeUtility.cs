namespace MyGameName;
using CLSS;
using EnumFastToStringGenerated;
using Godot;
using MyGameName.Enums;
using MyGameName.ValueObjects.Time;
using System;
using System.Linq;

public static class NodeUtility {
  private static readonly MemoizedFunc<Node, bool, Type, Node> _getFirstChildOfType = MemoizedFunc<Node, bool, Type, Node>.From(GetFirstChildOfType_Private);

  #region methods
  public static void AddToGroup(this Node node, GodotGroup godotGroup) => node.AddToGroup(godotGroup.ToStringFast(), false);

  public static void CallGroup(this Node node, GodotGroup group, StringName method, params Variant[] args) => node.GetTree().CallGroup(group.ToStringFast(), method, args);

  public static TNode GetFirstChildOfType<TNode>(this Node node, bool includeInternal = false) where TNode : Node => (TNode)_getFirstChildOfType.Invoke(node, includeInternal, typeof(TNode));

  public static T GetNode<T>(this Node node, NodePath path) where T : Node => (T)node.GetNode(path);

  public static TNode GetOrAddChildOfType<TNode>(this Node node) where TNode : Node, new() {
    TNode childNode;
    try {
      childNode = (TNode)_getFirstChildOfType.Invoke(node, true, typeof(TNode));
    }
    catch (InvalidOperationException) {
      // Was not found, so add it
      childNode = new TNode {
        Name = typeof(TNode).Name
      };
      node.AddChild(childNode, false, Node.InternalMode.Front);
      Logger.Log($"Added child node {childNode.Name}");
    }
    return childNode;
  }

  public static void RemoveFromGroup(this Node node, GodotGroup godotGroup) => node.RemoveFromGroup(godotGroup.ToStringFast());

  public static SignalAwaiter TimerFromSelf(this Node node, Duration duration) => node.ToSignal(node.GetTree().CreateTimer((double)duration), SceneTreeTimer.SignalName.Timeout);

  public static SignalAwaiter TimerFromSelf(this Node node, TimeSpan duration) => node.ToSignal(node.GetTree().CreateTimer(duration.TotalSeconds), SceneTreeTimer.SignalName.Timeout);

  private static Node GetFirstChildOfType_Private(Node node, bool includeInternal, Type type) => node.GetChildren(includeInternal).First(type.IsInstanceOfType);
  #endregion
}

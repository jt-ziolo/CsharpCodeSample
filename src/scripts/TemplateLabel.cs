namespace MyGameName;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using CLSS;
using Godot;
using Scriban;
using Scriban.Syntax;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class TemplateLabel : RichTextLabel {
  #region properties

  [Dependency]
  public TemplateLabelLookupService Lookup => this.DependOn<TemplateLabelLookupService>();

  private Template? Template { get; set; }
  private TemplateContext? TemplateContext { get; set; }
  #endregion

  public override void _Process(double delta) {
    string renderResult;
    try {
      renderResult = Template!.Render(TemplateContext);
    }
    catch {
      renderResult = Text;
      // Logger.LogWarning($"Could not find variable names in template for {Text}");
    }
    Text = renderResult;
  }

  public void OnResolved() {
    TemplateContext = new TemplateContext();
    TemplateContext.PushGlobal(Lookup.ScriptObject);
    Template = Template.Parse(Text);
  }
}

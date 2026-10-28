using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace ChecklistsModern.Filters;

// MVC 5 never rendered maxlength from [StringLength]; with it the browser truncates input and the
// legacy "maximum length" validation message can no longer be reached from the screen.
public class LegacyHtmlGenerator : DefaultHtmlGenerator
{
    public LegacyHtmlGenerator(
        IAntiforgery antiforgery,
        IOptions<MvcViewOptions> optionsAccessor,
        IModelMetadataProvider metadataProvider,
        IUrlHelperFactory urlHelperFactory,
        HtmlEncoder htmlEncoder,
        ValidationHtmlAttributeProvider validationAttributeProvider)
        : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider)
    {
    }

    public override TagBuilder GenerateTextBox(ViewContext viewContext, ModelExplorer modelExplorer, string expression,
        object value, string format, object htmlAttributes) =>
        WithoutMaxLength(base.GenerateTextBox(viewContext, modelExplorer, expression, value, format, htmlAttributes), htmlAttributes);

    public override TagBuilder GeneratePassword(ViewContext viewContext, ModelExplorer modelExplorer, string expression,
        object value, object htmlAttributes) =>
        WithoutMaxLength(base.GeneratePassword(viewContext, modelExplorer, expression, value, htmlAttributes), htmlAttributes);

    private static TagBuilder WithoutMaxLength(TagBuilder tagBuilder, object htmlAttributes)
    {
        var explicitAttributes = htmlAttributes as IDictionary<string, object?> ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
        if (!explicitAttributes.ContainsKey("maxlength"))
            tagBuilder.Attributes.Remove("maxlength");
        return tagBuilder;
    }
}

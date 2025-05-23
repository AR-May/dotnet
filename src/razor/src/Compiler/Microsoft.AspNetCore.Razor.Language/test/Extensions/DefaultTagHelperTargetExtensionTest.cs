﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Xunit;
using static Microsoft.AspNetCore.Razor.Language.CommonMetadata;

namespace Microsoft.AspNetCore.Razor.Language.Extensions;

public class DefaultTagHelperTargetExtensionTest : RazorProjectEngineTestBase
{
    protected override RazorLanguageVersion Version => RazorLanguageVersion.Latest;

    private static readonly TagHelperDescriptor StringPropertyTagHelper = CreateTagHelperDescriptor(
        tagName: "input",
        typeName: "InputTagHelper",
        assemblyName: "TestAssembly",
        attributes: new Action<BoundAttributeDescriptorBuilder>[]
        {
            builder => builder
                .Name("bound")
                .Metadata(PropertyName("StringProp"))
                .TypeName("System.String"),
        });

    private static readonly TagHelperDescriptor IntPropertyTagHelper = CreateTagHelperDescriptor(
        tagName: "input",
        typeName: "InputTagHelper",
        assemblyName: "TestAssembly",
        attributes: new Action<BoundAttributeDescriptorBuilder>[]
        {
            builder => builder
                .Name("bound")
                .Metadata(PropertyName("IntProp"))
                .TypeName("System.Int32"),
        });

    private static readonly TagHelperDescriptor IntIndexerTagHelper = CreateTagHelperDescriptor(
        tagName: "input",
        typeName: "InputTagHelper",
        assemblyName: "TestAssembly",
        attributes: new Action<BoundAttributeDescriptorBuilder>[]
        {
            builder => builder
                .Name("bound")
                .Metadata(PropertyName("IntIndexer"))
                .TypeName("System.Collections.Generic.Dictionary<System.String, System.Int32>")
                .AsDictionary("foo-", "System.Int32"),
        });

    private static readonly SourceSpan Span = new SourceSpan("test.cshtml", 15, 2, 5, 2);

    [Fact]
    public void WriteTagHelperBody_DesignTime_WritesChildren()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperBodyIntermediateNode()
        {
            Children =
                {
                    new CSharpExpressionIntermediateNode(),
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperBody(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"Render Children
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperBody_Runtime_RendersCorrectly_UsesTagNameAndModeFromContext()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperBodyIntermediateNode()
        {
            Children =
                {
                    new CSharpExpressionIntermediateNode(),
                },
            TagMode = TagMode.SelfClosing,
            TagName = "p",
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperBody(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__tagHelperExecutionContext = __tagHelperScopeManager.Begin(""p"", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, ""test"", async() => {
    Render Children
}
);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperCreate_DesignTime_RendersCorrectly_UsesSpecifiedTagHelperType()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperCreateIntermediateNode()
        {
            FieldName = "__TestNamespace_MyTagHelper",
            TypeName = "TestNamespace.MyTagHelper",
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperCreate(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__TestNamespace_MyTagHelper = CreateTagHelper<global::TestNamespace.MyTagHelper>();
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperCreate_Runtime_RendersCorrectly_UsesSpecifiedTagHelperType()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperCreateIntermediateNode()
        {
            FieldName = "__TestNamespace_MyTagHelper",
            TypeName = "TestNamespace.MyTagHelper",
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperCreate(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__TestNamespace_MyTagHelper = CreateTagHelper<global::TestNamespace.MyTagHelper>();
__tagHelperExecutionContext.Add(__TestNamespace_MyTagHelper);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperExecute_DesignTime_RendersAsyncCode()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperExecuteIntermediateNode();
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperExecute(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
            @"await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperExecute_Runtime_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperExecuteIntermediateNode();
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperExecute(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
if (!__tagHelperExecutionContext.Output.IsContentModified)
{
    await __tagHelperExecutionContext.SetOutputContentAsync();
}
Write(__tagHelperExecutionContext.Output);
__tagHelperExecutionContext = __tagHelperScopeManager.End();
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperHtmlAttribute_DesignTime_WritesNothing()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperHtmlAttributeIntermediateNode()
        {
            AttributeName = "name",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            Children =
                {
                    new HtmlAttributeValueIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.Html, Content = "Blah-" } }
                    },
                    new CSharpCodeAttributeValueIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "\"Foo\"", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperHtmlAttribute(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"Render Children
Render Children
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperHtmlAttribute_Runtime_SimpleAttribute_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperHtmlAttributeIntermediateNode()
        {
            AttributeName = "name",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            Children =
                {
                    new HtmlAttributeIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.Html, Content = "\"value\"", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperHtmlAttribute(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"BeginWriteTagHelperAttribute();
Render Children
__tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
__tagHelperExecutionContext.AddHtmlAttribute(""name"", Html.Raw(__tagHelperStringValueBuffer), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperHtmlAttribute_Runtime_DynamicAttribute_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperHtmlAttributeIntermediateNode()
        {
            AttributeName = "name",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            Children =
                {
                    new HtmlAttributeValueIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.Html, Content = "Blah-" } }
                    },
                    new CSharpCodeAttributeValueIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "\"Foo\"", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperHtmlAttribute(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"BeginAddHtmlAttributeValues(__tagHelperExecutionContext, ""name"", 2, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
Render Children
Render Children
EndAddHtmlAttributeValues(__tagHelperExecutionContext);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void RenderTagHelperAttributeInline_NonString_StatementInAttribute_Errors()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            IsIndexerNameMatch = false,
        };
        var expectedLocation = new SourceSpan(100, 10);
        var expectedDiagnostic = RazorDiagnosticFactory.CreateTagHelper_CodeBlocksNotSupportedInAttributes(expectedLocation);

        // Act
        extension.RenderTagHelperAttributeInline(context, node, new CSharpCodeIntermediateNode(), expectedLocation);

        // Assert
        var diagnostic = Assert.Single(context.GetDiagnostics());
        Assert.Equal(expectedDiagnostic, diagnostic);
    }

    [Fact]
    public void RenderTagHelperAttributeInline_NonStringIndexerMatch_TemplateInAttribute_Errors()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            IsIndexerNameMatch = true,
        };
        var expectedLocation = new SourceSpan(100, 10);
        var expectedDiagnostic = RazorDiagnosticFactory.CreateTagHelper_InlineMarkupBlocksNotSupportedInAttributes(expectedLocation, "System.Int32");

        // Act
        extension.RenderTagHelperAttributeInline(context, node, new TemplateIntermediateNode(), expectedLocation);

        // Assert
        var diagnostic = Assert.Single(context.GetDiagnostics());
        Assert.Equal(expectedDiagnostic, diagnostic);
    }

    [Fact]
    public void RenderTagHelperAttributeInline_NonString_TemplateInAttribute_Errors()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            IsIndexerNameMatch = false,
        };
        var expectedLocation = new SourceSpan(100, 10);
        var expectedDiagnostic = RazorDiagnosticFactory.CreateTagHelper_InlineMarkupBlocksNotSupportedInAttributes(
            expectedLocation,
            "System.Collections.Generic.Dictionary<System.String, System.Int32>");

        // Act
        extension.RenderTagHelperAttributeInline(context, node, new TemplateIntermediateNode(), expectedLocation);

        // Assert
        var diagnostic = Assert.Single(context.GetDiagnostics());
        Assert.Equal(expectedDiagnostic, diagnostic);
    }

    [Fact]
    public void WriteTagHelperProperty_DesignTime_StringProperty_HtmlContent_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = StringPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "StringProp",
            TagHelper = StringPropertyTagHelper,
            Children =
                {
                    new HtmlContentIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.Html, Content = "value", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"Render Children
__InputTagHelper.StringProp = ""value"";
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact] // We don't actually assign the expression result at design time, we just use string.Empty as a placeholder.
    public void WriteTagHelperProperty_DesignTime_StringProperty_NonHtmlContent_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = StringPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "StringProp",
            TagHelper = StringPropertyTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "\"3+5\"", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"Render Children
__InputTagHelper.StringProp = string.Empty;
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_DesignTime_NonStringProperty_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "IntProp",
            TagHelper = IntPropertyTagHelper,
            Source = Span,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"
#nullable restore
#line 3 ""test.cshtml""
__InputTagHelper.IntProp = 32;

#line default
#line hidden
#nullable disable
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    // If a value is bound to multiple tag helpers, we want to make sure to only render the first
    // occurrence of the expression due to side-effects.
    [Fact]
    public void WriteTagHelperProperty_DesignTime_NonStringProperty_SecondUseOfAttribute()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node1 = new DefaultTagHelperPropertyIntermediateNode()
        {
            // We only look at the attribute name here.
            AttributeName = "bound",
            FieldName = "__OtherTagHelper",
            PropertyName = "IntProp",
        };
        var node2 = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "IntProp",
            TagHelper = IntPropertyTagHelper,
            Source = Span,
        };
        tagHelperNode.Children.Add(node1);
        tagHelperNode.Children.Add(node2);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node2);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntProp = __OtherTagHelper.IntProp;
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_DesignTime_NonStringProperty_RendersCorrectly_WithoutLocation()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "IntProp",
            TagHelper = IntPropertyTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntProp = 32;
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_DesignTime_NonStringIndexer_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "foo-bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = true,
            PropertyName = "IntIndexer",
            TagHelper = IntIndexerTagHelper,
            Source = Span,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"
#nullable restore
#line 3 ""test.cshtml""
__InputTagHelper.IntIndexer[""bound""] = 32;

#line default
#line hidden
#nullable disable
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_DesignTime_NonStringIndexer_RendersCorrectly_WithoutLocation()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "foo-bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = true,
            PropertyName = "IntIndexer",
            TagHelper = IntIndexerTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntIndexer[""bound""] = 32;
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_Runtime_StringProperty_HtmlContent_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = StringPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "StringProp",
            TagHelper = StringPropertyTagHelper,
            Children =
                {
                    new HtmlContentIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.Html, Content = "\"value\"", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();

        // The attribute value is not rendered inline because we are not using the preallocated writer.
        Assert.Equal(
@"BeginWriteTagHelperAttribute();
Render Children
__tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
__InputTagHelper.StringProp = __tagHelperStringValueBuffer;
__tagHelperExecutionContext.AddTagHelperAttribute(""bound"", __InputTagHelper.StringProp, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_Runtime_NonStringProperty_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "IntProp",
            TagHelper = IntPropertyTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", Source = Span } },
                    }
                },
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntProp = 
#nullable restore
#line (3,6)-(3,1) ""test.cshtml""
32

#line default
#line hidden
#nullable disable
;
__tagHelperExecutionContext.AddTagHelperAttribute(""bound"", __InputTagHelper.IntProp, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    // If a value is bound to multiple tag helpers, we want to make sure to only render the first
    // occurrence of the expression due to side-effects.
    [Fact]
    public void WriteTagHelperProperty_Runtime_NonStringProperty_SecondUseOfAttribute()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node1 = new DefaultTagHelperPropertyIntermediateNode()
        {
            // We only look at the attribute name here.
            AttributeName = "bound",
            FieldName = "__OtherTagHelper",
            PropertyName = "IntProp",
        };
        var node2 = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "IntProp",
            TagHelper = IntPropertyTagHelper,
            Source = Span,
        };
        tagHelperNode.Children.Add(node1);
        tagHelperNode.Children.Add(node2);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node2);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntProp = __OtherTagHelper.IntProp;
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_Runtime_NonStringProperty_RendersCorrectly_WithoutLocation()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntPropertyTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = false,
            PropertyName = "IntProp",
            TagHelper = IntPropertyTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntProp = 32;
__tagHelperExecutionContext.AddTagHelperAttribute(""bound"", __InputTagHelper.IntProp, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_Runtime_NonStringIndexer_RendersCorrectly()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "foo-bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = true,
            PropertyName = "IntIndexer",
            TagHelper = IntIndexerTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", Source = Span } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"if (__InputTagHelper.IntIndexer == null)
{
    throw new InvalidOperationException(InvalidTagHelperIndexerAssignment(""foo-bound"", ""InputTagHelper"", ""IntIndexer""));
}
__InputTagHelper.IntIndexer[""bound""] = 
#nullable restore
#line (3,6)-(3,1) ""test.cshtml""
32

#line default
#line hidden
#nullable disable
;
__tagHelperExecutionContext.AddTagHelperAttribute(""foo-bound"", __InputTagHelper.IntIndexer[""bound""], global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact] // We should only emit the validation code for the first use of an indexer property.
    public void WriteTagHelperProperty_Runtime_NonStringIndexer_MultipleValues()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node1 = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "foo-first",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = true,
            PropertyName = "IntIndexer",
            TagHelper = IntIndexerTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "17", Source = Span } },
                    }
                }
        };
        var node2 = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "foo-bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = true,
            PropertyName = "IntIndexer",
            TagHelper = IntIndexerTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", Source = Span } },
                    }
                }
        };
        tagHelperNode.Children.Add(node1);
        tagHelperNode.Children.Add(node2);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node2);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"__InputTagHelper.IntIndexer[""bound""] = 
#nullable restore
#line (3,6)-(3,1) ""test.cshtml""
32

#line default
#line hidden
#nullable disable
;
__tagHelperExecutionContext.AddTagHelperAttribute(""foo-bound"", __InputTagHelper.IntIndexer[""bound""], global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperProperty_Runtime_NonStringIndexer_RendersCorrectly_WithoutLocation()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var tagHelperNode = new TagHelperIntermediateNode();
        var node = new DefaultTagHelperPropertyIntermediateNode()
        {
            AttributeName = "foo-bound",
            AttributeStructure = AttributeStructure.DoubleQuotes,
            BoundAttribute = IntIndexerTagHelper.BoundAttributes.Single(),
            FieldName = "__InputTagHelper",
            IsIndexerNameMatch = true,
            PropertyName = "IntIndexer",
            TagHelper = IntIndexerTagHelper,
            Children =
                {
                    new CSharpExpressionIntermediateNode()
                    {
                        Children = { new IntermediateToken { Kind = TokenKind.CSharp, Content = "32", } },
                    }
                }
        };
        tagHelperNode.Children.Add(node);
        Push(context, tagHelperNode);

        // Act
        extension.WriteTagHelperProperty(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"if (__InputTagHelper.IntIndexer == null)
{
    throw new InvalidOperationException(InvalidTagHelperIndexerAssignment(""foo-bound"", ""InputTagHelper"", ""IntIndexer""));
}
__InputTagHelper.IntIndexer[""bound""] = 32;
__tagHelperExecutionContext.AddTagHelperAttribute(""foo-bound"", __InputTagHelper.IntIndexer[""bound""], global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperRuntime_DesignTime_RendersPreRequisites()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateDesignTime();

        var node = new DefaultTagHelperRuntimeIntermediateNode();

        // Act
        extension.WriteTagHelperRuntime(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
            @"#line hidden
#pragma warning disable 0649
private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
#pragma warning restore 0649
private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void WriteTagHelperRuntime_Runtime_DeclaresRequiredFields()
    {
        // Arrange
        var extension = new DefaultTagHelperTargetExtension();
        using var context = TestCodeRenderingContext.CreateRuntime();

        var node = new DefaultTagHelperRuntimeIntermediateNode();

        // Act
        extension.WriteTagHelperRuntime(context, node);

        // Assert
        var csharp = context.CodeWriter.GetText().ToString();
        Assert.Equal(
@"#line hidden
#pragma warning disable 0649
private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
#pragma warning restore 0649
private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
#pragma warning disable 0169
private string __tagHelperStringValueBuffer;
#pragma warning restore 0169
private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
{
    get
    {
        if (__backed__tagHelperScopeManager == null)
        {
            __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
        }
        return __backed__tagHelperScopeManager;
    }
}
",
            csharp,
            ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void GetDeterministicId_IsDeterministic()
    {
        // Arrange
        using var context = TestCodeRenderingContext.CreateRuntime(suppressUniqueIds: null);

        // Act
        var firstId = DefaultTagHelperTargetExtension.GetDeterministicId(context);
        var secondId = DefaultTagHelperTargetExtension.GetDeterministicId(context);

        // Assert
        Assert.Equal(firstId, secondId);
    }

    private static void Push(CodeRenderingContext context, TagHelperIntermediateNode node)
    {
        context.PushAncestor(node);
    }

    private static TagHelperDescriptor CreateTagHelperDescriptor(
        string tagName,
        string typeName,
        string assemblyName,
        IEnumerable<Action<BoundAttributeDescriptorBuilder>> attributes = null)
    {
        var builder = TagHelperDescriptorBuilder.Create(typeName, assemblyName);
        builder.Metadata(TypeName(typeName));

        if (attributes != null)
        {
            foreach (var attributeBuilder in attributes)
            {
                builder.BindAttribute(attributeBuilder);
            }
        }

        builder.TagMatchingRule(ruleBuilder => ruleBuilder.RequireTagName(tagName));

        var descriptor = builder.Build();

        return descriptor;
    }
}

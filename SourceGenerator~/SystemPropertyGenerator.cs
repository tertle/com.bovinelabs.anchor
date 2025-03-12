namespace BovineLabs.SourceGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using BovineLabs.SourceGenerator.Extensions;
    using CodeGenHelpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator]
    public class SystemPropertyGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            try
            {
                var contextProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(predicate: IsContextSyntaxTargetForGeneration, transform: GetSemanticTargetForGeneration);

                context.RegisterSourceOutput(contextProvider, (productionContext, fieldData) =>
                {
                    try
                    {
                        var builder = ProcessField(fieldData);
                        if (builder == null)
                        {
                            return;
                        }

                        var text = Microsoft.CodeAnalysis.Text.SourceText.From(builder.Build(), Encoding.UTF8);
                        productionContext.AddSource($"{builder.FullyQualifiedName}.{fieldData.PropertyName}.g.cs", text);
                    }
                    catch (Exception ex)
                    {
                        SourceGenHelpers.Log($"Exception occured: {ex.Message}\n{ex.StackTrace}");
                    }
                });
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log($"Exception occured: {ex}");
            }
        }

        public static bool IsContextSyntaxTargetForGeneration(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Is Struct
                if (syntaxNode is not FieldDeclarationSyntax fieldDeclarationSyntax)
                    return false;

                if (!fieldDeclarationSyntax.HasAttribute("SystemPropertyAttribute"))
                {
                    return false;
                }

                var parent = GetEnclosingStruct(fieldDeclarationSyntax);
                if (parent == null)
                {
                    return false;
                }

                // Has Partial keyword
                var hasPartial = false;
                foreach (var m in parent.Modifiers)
                {
                    if (m.IsKind(SyntaxKind.PartialKeyword))
                    {
                        hasPartial = true;

                        break;
                    }
                }

                if (!hasPartial)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log($"Exception occured: {ex}");
                return false;
            }
        }

        public static FieldData GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
        {
            var fieldDeclarationSyntax = (FieldDeclarationSyntax)ctx.Node;
            StructDeclarationSyntax structDeclarationSyntax = GetEnclosingStruct(fieldDeclarationSyntax);

            var namespaces = ctx.SemanticModel.GetAccessibleNamespaces(structDeclarationSyntax);
            INamedTypeSymbol typeSymbol = ctx.SemanticModel.GetDeclaredSymbol(structDeclarationSyntax);

            var ancestors = structDeclarationSyntax.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Select(classSyntax => ctx.SemanticModel.GetDeclaredSymbol(classSyntax))
                .Reverse().ToArray();

            return new FieldData(typeSymbol, ancestors, namespaces, fieldDeclarationSyntax);
        }

        public static StructDeclarationSyntax GetEnclosingStruct(FieldDeclarationSyntax fieldDeclaration)
        {
            // Get the parent node of the field declaration
            SyntaxNode parent = fieldDeclaration.Parent;
            return parent as StructDeclarationSyntax;
        }

        private static ClassBuilder ProcessField(FieldData fieldData)
        {
            try
            {
                ClassBuilder builder;

                if (fieldData.Ancestors.Length == 0)
                {
                    builder = CodeBuilder
                        .Create(fieldData.TypeSymbol);
                }
                else
                {
                    builder = CodeBuilder.Create(fieldData.Ancestors[0]);

                    for (var i = 1; i < fieldData.Ancestors.Length; i++)
                    {
                        builder = builder.AddNestedClass(fieldData.Ancestors[i]);
                    }

                    builder = builder.AddNestedClass(fieldData.TypeSymbol);
                }

                // Add all namespaces from the context
                foreach (var namespaces in fieldData.Namespaces)
                {
                     builder.AddNamespaceImport(namespaces);
                }

                builder.AddProperty(fieldData.PropertyName, Accessibility.Public)
                    .SetType(fieldData.FieldType)
                    .WithGetterExpression($"this.{fieldData.FieldName}")
                    .WithSetterExpression($"this.SetProperty(ref {fieldData.FieldName}, value)");

                return builder;
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log(ex.ToString());
                return null;
            }
        }
    }
}

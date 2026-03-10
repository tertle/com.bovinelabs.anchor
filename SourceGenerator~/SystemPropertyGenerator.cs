// <copyright file="SystemPropertyGenerator.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SystemPropertyGenerator
{
    using System.Collections.Immutable;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using BovineLabs.SystemPropertyGenerator.Extensions;
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

                context.RegisterSourceOutput(contextProvider, (productionContext, fieldResult) =>
                {
                    try
                    {
                        if (fieldResult == null)
                        {
                            return;
                        }

                        foreach (var diagnostic in fieldResult.Diagnostics)
                        {
                            productionContext.ReportDiagnostic(diagnostic);
                        }

                        if (fieldResult.FieldData == null)
                        {
                            return;
                        }

                        var builder = ProcessField(fieldResult.FieldData);
                        if (builder == null)
                        {
                            return;
                        }

                        var text = Microsoft.CodeAnalysis.Text.SourceText.From(builder.Build(), Encoding.UTF8);
                        productionContext.AddSource($"{builder.FullyQualifiedName}.{fieldResult.FieldData.PropertyName}.g.cs", text);
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
                if (syntaxNode is not FieldDeclarationSyntax fieldDeclarationSyntax)
                {
                    return false;
                }

                return fieldDeclarationSyntax.HasAttribute("SystemPropertyAttribute");
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log($"Exception occured: {ex}");
                return false;
            }
        }

        public static FieldResult GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
        {
            var fieldDeclarationSyntax = (FieldDeclarationSyntax)ctx.Node;
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            if (fieldDeclarationSyntax.Declaration.Variables.Count != 1)
            {
                var firstName = fieldDeclarationSyntax.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? fieldDeclarationSyntax.Declaration.Type.ToString();
                diagnostics.Add(SystemPropertyDiagnostics.InvalidFieldDeclaration(firstName, fieldDeclarationSyntax.Declaration.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            var variable = fieldDeclarationSyntax.Declaration.Variables[0];
            if (ctx.SemanticModel.GetDeclaredSymbol(variable, cancellationToken) is not IFieldSymbol fieldSymbol)
            {
                return null;
            }

            StructDeclarationSyntax structDeclarationSyntax = GetEnclosingStruct(fieldDeclarationSyntax);
            if (structDeclarationSyntax == null)
            {
                diagnostics.Add(SystemPropertyDiagnostics.NotInStruct(fieldSymbol, variable.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            if (!IsPartial(structDeclarationSyntax))
            {
                if (ctx.SemanticModel.GetDeclaredSymbol(structDeclarationSyntax, cancellationToken) is INamedTypeSymbol nonPartialType)
                {
                    diagnostics.Add(SystemPropertyDiagnostics.MissingPartial(nonPartialType, structDeclarationSyntax.Identifier.GetLocation()));
                }

                return new FieldResult(null, diagnostics.ToImmutable());
            }

            if (fieldSymbol.IsStatic || fieldSymbol.IsConst || fieldSymbol.IsReadOnly)
            {
                diagnostics.Add(SystemPropertyDiagnostics.InvalidFieldDeclaration(fieldSymbol.Name, variable.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            var namespaces = ctx.SemanticModel.GetAccessibleNamespaces(structDeclarationSyntax);
            INamedTypeSymbol typeSymbol = ctx.SemanticModel.GetDeclaredSymbol(structDeclarationSyntax);

            var ancestors = structDeclarationSyntax.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Select(classSyntax => ctx.SemanticModel.GetDeclaredSymbol(classSyntax))
                .Reverse().ToArray();

            return new FieldResult(new FieldData(typeSymbol, ancestors, namespaces, fieldDeclarationSyntax), diagnostics.ToImmutable());
        }

        public static StructDeclarationSyntax GetEnclosingStruct(FieldDeclarationSyntax fieldDeclaration)
        {
            // Get the parent node of the field declaration
            SyntaxNode parent = fieldDeclaration.Parent;
            return parent as StructDeclarationSyntax;
        }

        private static bool IsPartial(TypeDeclarationSyntax declaration)
        {
            foreach (var modifier in declaration.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        private static ClassBuilder ProcessField(FieldData fieldData)
        {
            try
            {
                var builder = CreateClassBuilder(fieldData);

                // Add all namespaces from the context
                foreach (var namespaces in fieldData.Namespaces)
                {
                    builder.AddNamespaceImport(namespaces);
                }

                if (fieldData.FieldMode != FieldMode.NativeList)
                {
                    builder.AddProperty(fieldData.PropertyName, Accessibility.Public)
                        .SetType($"{fieldData.FieldType}")
                        .WithGetterExpression($"this.{fieldData.FieldName}")
                        .WithSetterExpression($"this.SetProperty(ref {fieldData.FieldName}, value)");
                }

                switch (fieldData.FieldMode)
                {
                    case FieldMode.NativeList:
                        builder.AddProperty(fieldData.PropertyName, Accessibility.Public)
                            .SetType($"MultiContainer<{fieldData.GenericType}>")
                            .WithGetterExpression($"this.{fieldData.FieldName}")
                            .WithSetterExpression($"this.SetProperty({fieldData.FieldName}, value)");
                        break;

                    case FieldMode.Changed:
                    {
                        builder.AddMethod($"{fieldData.PropertyName}Changed", Accessibility.Public)
                            .WithReturnType("bool")
                            .AddParameter($"out {fieldData.GenericType}", "value")
                            .AddParameter("bool", "resetToDefault = false")
                            .WithBody(writer =>
                            {
                                writer.AppendLine($"value = this.{fieldData.FieldName}.Value;");
                                writer.If($"this.{fieldData.FieldName}.HasChanged").WithBody(ifWriter =>
                                    {
                                        ifWriter
                                            .If("resetToDefault").WithBody(rw =>
                                            {
                                                rw.AppendLine($"this.{fieldData.PropertyName} = new {fieldData.FieldType}(default, false);");
                                            })
                                            .Else().WithBody(rw =>
                                            {
                                                rw.AppendLine(
                                                    $"this.{fieldData.FieldName} = new {fieldData.FieldType}(this.{fieldData.FieldName}.Value, false);");
                                            })
                                            .EndIf();

                                        ifWriter.AppendLine("return true;");
                                    })
                                    .EndIf();

                                writer.AppendLine("return false;");
                            });
                        break;
                    }

                    case FieldMode.ChangedList:
                    case FieldMode.Default:
                    default:
                        break;
                }

                return builder;
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log(ex.ToString());
                return null;
            }
        }

        private static ClassBuilder CreateClassBuilder(FieldData fieldData)
        {
            var classSymbol = fieldData.Ancestors.Length == 0 ? fieldData.TypeSymbol : fieldData.Ancestors[0];

            var builder = CodeBuilder
                .Create(classSymbol)
                .AddNamespaceImport("BovineLabs.Anchor.Binding");

            // Not nested, just return
            if (fieldData.Ancestors.Length <= 0)
            {
                return builder;
            }

            // Support nested classes
            for (var i = 1; i < fieldData.Ancestors.Length; i++)
            {
                builder = builder.AddNestedClass(fieldData.Ancestors[i]);
            }

            builder = builder.AddNestedClass(fieldData.TypeSymbol);
            return builder;
        }

        public sealed class FieldResult
        {
            public FieldResult(FieldData fieldData, ImmutableArray<Diagnostic> diagnostics)
            {
                this.FieldData = fieldData;
                this.Diagnostics = diagnostics;
            }

            public FieldData FieldData { get; }

            public ImmutableArray<Diagnostic> Diagnostics { get; }
        }
    }
}

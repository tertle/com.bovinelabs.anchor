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
        private const string AttributeMetadataName = "BovineLabs.Anchor.SystemPropertyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            try
            {
                // Roslyn supplies one node per variable in an attributed field declaration.
                var contextProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                    AttributeMetadataName,
                    static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax } declaration } &&
                        declaration.Variables[0].SpanStart == node.SpanStart,
                    GetSemanticTargetForGeneration);

                context.RegisterSourceOutput(contextProvider, (productionContext, fieldResult) =>
                {
                    try
                    {
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

        private static FieldResult GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var variable = (VariableDeclaratorSyntax)ctx.TargetNode;
            var fieldDeclarationSyntax = (FieldDeclarationSyntax)variable.Parent.Parent;
            var fieldSymbol = (IFieldSymbol)ctx.TargetSymbol;
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            if (fieldDeclarationSyntax.Declaration.Variables.Count != 1)
            {
                diagnostics.Add(SystemPropertyDiagnostics.InvalidFieldDeclaration(fieldSymbol.Name, fieldDeclarationSyntax.Declaration.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            if (fieldDeclarationSyntax.Parent is not StructDeclarationSyntax structDeclarationSyntax)
            {
                diagnostics.Add(SystemPropertyDiagnostics.NotInStruct(fieldSymbol, variable.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            if (!IsPartial(structDeclarationSyntax))
            {
                diagnostics.Add(SystemPropertyDiagnostics.MissingPartial(fieldSymbol.ContainingType, structDeclarationSyntax.Identifier.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            if (fieldSymbol.IsStatic || fieldSymbol.IsConst || fieldSymbol.IsReadOnly)
            {
                diagnostics.Add(SystemPropertyDiagnostics.InvalidFieldDeclaration(fieldSymbol.Name, variable.GetLocation()));
                return new FieldResult(null, diagnostics.ToImmutable());
            }

            var namespaces = ctx.SemanticModel.GetAccessibleNamespaces(structDeclarationSyntax);
            var ancestors = structDeclarationSyntax.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Select(classSyntax => ctx.SemanticModel.GetDeclaredSymbol(classSyntax, cancellationToken))
                .Reverse().ToArray();

            return new FieldResult(new FieldData(fieldSymbol.ContainingType, ancestors, namespaces, fieldDeclarationSyntax), diagnostics.ToImmutable());
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
                FieldData = fieldData;
                Diagnostics = diagnostics;
            }

            public FieldData FieldData { get; }

            public ImmutableArray<Diagnostic> Diagnostics { get; }
        }
    }
}

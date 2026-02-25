// <copyright file="MvvmGenerator.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SystemPropertyGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using CodeGenHelpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator]
    public sealed class MvvmGenerator : IIncrementalGenerator
    {
        private const string ObservablePropertyAttributeName = "ObservableProperty";
        private const string ICommandAttributeName = "ICommand";
        private const string AlsoNotifyChangeForAttributeName = "AlsoNotifyChangeFor";
        private const string CommandTypeName = "global::System.Windows.Input.ICommand";
        private const string RelayCommandTypeName = "global::BovineLabs.Anchor.MVVM.RelayCommand";

        private static readonly SymbolDisplayFormat QualifiedTypeFormat =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            try
            {
                var candidates = context.SyntaxProvider
                    .CreateSyntaxProvider(predicate: IsSyntaxTargetForGeneration, transform: GetSemanticTargetForGeneration)
                    .Where(static candidate => candidate != null);

                context.RegisterSourceOutput(
                    candidates.Collect(),
                    static (productionContext, collectedCandidates) => Execute(productionContext, collectedCandidates));
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log($"MVVM generator initialization exception: {ex}");
            }
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syntaxNode is not TypeDeclarationSyntax typeDeclaration)
            {
                return false;
            }

            if (!IsPartial(typeDeclaration))
            {
                return false;
            }

            foreach (var member in typeDeclaration.Members)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (member)
                {
                    case FieldDeclarationSyntax fieldDeclaration:
                        if (HasAttributeSyntax(fieldDeclaration.AttributeLists, ObservablePropertyAttributeName))
                        {
                            return true;
                        }

                        break;
                    case MethodDeclarationSyntax methodDeclaration:
                        if (HasAttributeSyntax(methodDeclaration.AttributeLists, ICommandAttributeName))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        private static PartialTypeModel GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
        {
            var typeDeclaration = (TypeDeclarationSyntax)ctx.Node;
            if (ctx.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol typeSymbol)
            {
                return null;
            }

            var observableProperties = ImmutableArray.CreateBuilder<ObservablePropertyData>();
            var commands = ImmutableArray.CreateBuilder<CommandData>();

            CollectObservableProperties(typeDeclaration, ctx.SemanticModel, observableProperties, cancellationToken);
            CollectCommands(typeDeclaration, ctx.SemanticModel, commands, cancellationToken);

            if (observableProperties.Count == 0 && commands.Count == 0)
            {
                return null;
            }

            return new PartialTypeModel(typeSymbol, observableProperties.ToImmutable(), commands.ToImmutable());
        }

        private static void Execute(SourceProductionContext context, ImmutableArray<PartialTypeModel> candidates)
        {
            try
            {
                var models = MergeCandidates(candidates, context.CancellationToken);
                foreach (var model in models)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var builder = Generate(model);
                    if (builder == null)
                    {
                        continue;
                    }

                    var hintName = $"{SanitizeFileName(model.TypeSymbol.ToDisplayString(QualifiedTypeFormat))}.MVVM.g.cs";
                    context.AddSource(hintName, builder.Build());
                }
            }
            catch (Exception ex)
            {
                SourceGenHelpers.Log($"MVVM generator exception: {ex}");
            }
        }

        private static IReadOnlyCollection<TypeGenerationModel> MergeCandidates(
            ImmutableArray<PartialTypeModel> candidates,
            CancellationToken cancellationToken)
        {
            var models = new Dictionary<INamedTypeSymbol, TypeGenerationModel>(SymbolEqualityComparer.Default);

            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (candidate == null)
                {
                    continue;
                }

                if (!models.TryGetValue(candidate.TypeSymbol, out var model))
                {
                    model = new TypeGenerationModel(candidate.TypeSymbol);
                    models.Add(candidate.TypeSymbol, model);
                }

                foreach (var property in candidate.ObservableProperties)
                {
                    if (!model.ObservableProperties.ContainsKey(property.PropertyName))
                    {
                        model.ObservableProperties.Add(property.PropertyName, property);
                    }
                }

                foreach (var command in candidate.Commands)
                {
                    if (!model.Commands.ContainsKey(command.PropertyName))
                    {
                        model.Commands.Add(command.PropertyName, command);
                    }
                }
            }

            return models.Values;
        }

        private static void CollectObservableProperties(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            ICollection<ObservablePropertyData> properties,
            CancellationToken cancellationToken)
        {
            foreach (var fieldDeclaration in typeDeclaration.Members.OfType<FieldDeclarationSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (semanticModel.GetDeclaredSymbol(variable, cancellationToken) is not IFieldSymbol fieldSymbol)
                    {
                        continue;
                    }

                    if (!HasAttribute(fieldSymbol, ObservablePropertyAttributeName))
                    {
                        continue;
                    }

                    if (fieldSymbol.IsReadOnly || fieldSymbol.IsConst || fieldSymbol.IsStatic)
                    {
                        continue;
                    }

                    var propertyName = FormatPropertyName(fieldSymbol.Name);
                    if (string.IsNullOrWhiteSpace(propertyName) || properties.Any(p => p.PropertyName == propertyName))
                    {
                        continue;
                    }

                    var additionalNotifications = GetAlsoNotifyProperties(fieldSymbol);
                    var fieldTypeName = fieldSymbol.Type.ToDisplayString(QualifiedTypeFormat);
                    properties.Add(new ObservablePropertyData(fieldSymbol.Name, fieldTypeName, propertyName, additionalNotifications));
                }
            }
        }

        private static void CollectCommands(
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel,
            ICollection<CommandData> commands,
            CancellationToken cancellationToken)
        {
            foreach (var methodDeclaration in typeDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (!HasAttribute(methodSymbol, ICommandAttributeName))
                {
                    continue;
                }

                if (!methodSymbol.ReturnsVoid || methodSymbol.Parameters.Length > 1)
                {
                    continue;
                }

                if (methodSymbol.Parameters.Any(static p => p.RefKind != RefKind.None))
                {
                    continue;
                }

                var propertyName = $"{UppercaseFirst(methodSymbol.Name)}Command";
                if (commands.Any(c => c.PropertyName == propertyName))
                {
                    continue;
                }

                var canExecuteName = GetNamedStringArgument(methodSymbol, ICommandAttributeName, "CanExecuteMethod");
                IMethodSymbol canExecuteMethod = null;

                if (!string.IsNullOrWhiteSpace(canExecuteName))
                {
                    canExecuteMethod = ResolveCanExecuteMethod(methodSymbol.ContainingType, canExecuteName, methodSymbol.Parameters);
                }

                commands.Add(new CommandData(methodSymbol, propertyName, canExecuteMethod));
            }
        }

        private static CodeBuilder Generate(TypeGenerationModel model)
        {
            var namespaceName = model.TypeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : model.TypeSymbol.ContainingNamespace.ToDisplayString();

            var builder = namespaceName == null
                ? CodeBuilder.CreateInGlobalNamespace()
                : CodeBuilder.Create(namespaceName);

            var typeBuilder = AddTypeChain(builder, model.TypeSymbol);

            foreach (var property in model.ObservableProperties.Values.OrderBy(static p => p.PropertyName, StringComparer.Ordinal))
            {
                EmitObservableProperty(typeBuilder, property);
            }

            foreach (var command in model.Commands.Values.OrderBy(static c => c.PropertyName, StringComparer.Ordinal))
            {
                EmitCommand(typeBuilder, command);
            }

            return builder;
        }

        private static ClassBuilder AddTypeChain(CodeBuilder builder, INamedTypeSymbol typeSymbol)
        {
            var chain = GetTypeChain(typeSymbol);
            ClassBuilder classBuilder = null;

            for (var i = 0; i < chain.Count; i++)
            {
                classBuilder = i == 0
                    ? builder.AddClass(chain[i].Name)
                    : classBuilder.AddNestedClass(chain[i].Name, true, chain[i].DeclaredAccessibility);

                ConfigureTypeBuilder(classBuilder, chain[i]);
            }

            return classBuilder;
        }

        private static void ConfigureTypeBuilder(ClassBuilder classBuilder, INamedTypeSymbol symbol)
        {
            classBuilder.WithAccessModifier(symbol.DeclaredAccessibility);
            classBuilder.OfType(symbol.TypeKind);
            classBuilder.ReadOnly(symbol.IsReadOnly);

            if (symbol.TypeKind == TypeKind.Class)
            {
                if (symbol.IsStatic)
                {
                    classBuilder.MakeStaticClass();
                }
                else
                {
                    if (symbol.IsAbstract)
                    {
                        classBuilder.Abstract();
                    }

                    if (symbol.IsSealed)
                    {
                        classBuilder.Sealed();
                    }
                }
            }

            AddTypeParameters(classBuilder, symbol);
        }

        private static void AddTypeParameters(ClassBuilder classBuilder, INamedTypeSymbol symbol)
        {
            foreach (var typeParameter in symbol.TypeParameters)
            {
                classBuilder.AddGeneric(typeParameter.Name, genericBuilder =>
                {
                    if (typeParameter.HasReferenceTypeConstraint)
                    {
                        genericBuilder.Class();
                    }

                    if (typeParameter.HasUnmanagedTypeConstraint)
                    {
                        genericBuilder.Unmanaged();
                    }

                    if (typeParameter.HasValueTypeConstraint)
                    {
                        genericBuilder.AddConstraint("struct");
                    }

                    if (typeParameter.HasNotNullConstraint)
                    {
                        genericBuilder.AddConstraint("notnull");
                    }

                    foreach (var constraintType in typeParameter.ConstraintTypes)
                    {
                        genericBuilder.AddConstraint(constraintType.ToDisplayString(QualifiedTypeFormat));
                    }

                    if (typeParameter.HasConstructorConstraint)
                    {
                        genericBuilder.New();
                    }
                });
            }
        }

        private static void EmitObservableProperty(ClassBuilder classBuilder, ObservablePropertyData property)
        {
            var generatedProperty = classBuilder
                .AddProperty(property.PropertyName, Accessibility.Public)
                .SetType(property.FieldTypeName)
                .AddAttribute("global::Unity.Properties.CreateProperty");

            generatedProperty.WithGetterExpression($"this.{property.FieldName}");
            generatedProperty.WithSetter(setterWriter =>
            {
                setterWriter.If($"this.SetProperty(ref this.{property.FieldName}, value)").WithBody(ifWriter =>
                {
                    foreach (var notification in property.AdditionalNotifications)
                    {
                        ifWriter.AppendLine($"this.OnPropertyChanged(\"{Escape(notification)}\");");
                    }
                }).EndIf();
            });
        }

        private static void EmitCommand(ClassBuilder classBuilder, CommandData command)
        {
            var backingFieldName = $"__{LowercaseFirst(command.PropertyName)}";
            classBuilder
                .AddProperty(backingFieldName, Accessibility.Private)
                .SetType(CommandTypeName)
                .WithValue(null, CodeGenHelpers.ValueType.Null);

            classBuilder
                .AddProperty(command.PropertyName, Accessibility.Public)
                .SetType(CommandTypeName)
                .AddAttribute("global::Unity.Properties.CreateProperty(ReadOnly = true)")
                .WithGetterExpression($"this.{backingFieldName} ??= {BuildCommandExpression(command)}");
        }

        private static string BuildCommandExpression(CommandData command)
        {
            var method = command.MethodSymbol;
            var executeExpression = method.IsStatic
                ? $"{method.ContainingType.ToDisplayString(QualifiedTypeFormat)}.{method.Name}"
                : $"this.{method.Name}";

            var canExecuteExpression = BuildCanExecuteExpression(command);

            if (method.Parameters.Length == 0)
            {
                return canExecuteExpression == null
                    ? $"new {RelayCommandTypeName}({executeExpression})"
                    : $"new {RelayCommandTypeName}({executeExpression}, {canExecuteExpression})";
            }

            var parameterTypeName = method.Parameters[0].Type.ToDisplayString(QualifiedTypeFormat);
            return canExecuteExpression == null
                ? $"new {RelayCommandTypeName}<{parameterTypeName}>({executeExpression})"
                : $"new {RelayCommandTypeName}<{parameterTypeName}>({executeExpression}, {canExecuteExpression})";
        }

        private static string BuildCanExecuteExpression(CommandData command)
        {
            if (command.CanExecuteMethod == null)
            {
                return null;
            }

            return command.CanExecuteMethod.IsStatic
                ? $"{command.CanExecuteMethod.ContainingType.ToDisplayString(QualifiedTypeFormat)}.{command.CanExecuteMethod.Name}"
                : $"this.{command.CanExecuteMethod.Name}";
        }

        private static List<INamedTypeSymbol> GetTypeChain(INamedTypeSymbol typeSymbol)
        {
            var chain = new List<INamedTypeSymbol>();
            var cursor = typeSymbol;

            while (cursor != null)
            {
                chain.Add(cursor);
                cursor = cursor.ContainingType;
            }

            chain.Reverse();
            return chain;
        }

        private static IMethodSymbol ResolveCanExecuteMethod(
            INamedTypeSymbol typeSymbol,
            string methodName,
            ImmutableArray<IParameterSymbol> commandParameters)
        {
            foreach (var candidate in typeSymbol.GetMembers(methodName).OfType<IMethodSymbol>())
            {
                if (!candidate.ReturnType.SpecialType.Equals(SpecialType.System_Boolean))
                {
                    continue;
                }

                if (candidate.Parameters.Length != commandParameters.Length)
                {
                    continue;
                }

                if (candidate.Parameters.Any(static p => p.RefKind != RefKind.None))
                {
                    continue;
                }

                if (candidate.Parameters.Length == 1 &&
                    !SymbolEqualityComparer.Default.Equals(candidate.Parameters[0].Type, commandParameters[0].Type))
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static string GetNamedStringArgument(ISymbol symbol, string attributeName, string argumentName)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (!MatchesAttributeName(attribute.AttributeClass?.Name, attributeName))
                {
                    continue;
                }

                foreach (var pair in attribute.NamedArguments)
                {
                    if (!string.Equals(pair.Key, argumentName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    return pair.Value.Value as string;
                }
            }

            return null;
        }

        private static ImmutableArray<string> GetAlsoNotifyProperties(IFieldSymbol fieldSymbol)
        {
            var names = ImmutableArray.CreateBuilder<string>();

            foreach (var attribute in fieldSymbol.GetAttributes())
            {
                if (!MatchesAttributeName(attribute.AttributeClass?.Name, AlsoNotifyChangeForAttributeName))
                {
                    continue;
                }

                foreach (var argument in attribute.ConstructorArguments)
                {
                    if (argument.Kind == TypedConstantKind.Array)
                    {
                        foreach (var value in argument.Values)
                        {
                            if (value.Value is string propertyName && !string.IsNullOrWhiteSpace(propertyName))
                            {
                                names.Add(propertyName);
                            }
                        }
                    }
                    else if (argument.Value is string singlePropertyName && !string.IsNullOrWhiteSpace(singlePropertyName))
                    {
                        names.Add(singlePropertyName);
                    }
                }
            }

            return names.Distinct(StringComparer.Ordinal).ToImmutableArray();
        }

        private static bool HasAttribute(ISymbol symbol, string attributeName)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (MatchesAttributeName(attribute.AttributeClass?.Name, attributeName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttributeSyntax(SyntaxList<AttributeListSyntax> attributes, string attributeName)
        {
            foreach (var attributeList in attributes)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (MatchesAttributeName(attribute.Name.ToString(), attributeName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool MatchesAttributeName(string name, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(attributeName))
            {
                return false;
            }

            var simpleName = name;
            var dotIndex = simpleName.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < simpleName.Length - 1)
            {
                simpleName = simpleName.Substring(dotIndex + 1);
            }

            const string attributeSuffix = "Attribute";
            if (simpleName.EndsWith(attributeSuffix, StringComparison.Ordinal))
            {
                simpleName = simpleName.Substring(0, simpleName.Length - attributeSuffix.Length);
            }

            return string.Equals(simpleName, attributeName, StringComparison.Ordinal);
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

        private static string FormatPropertyName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            if (fieldName.StartsWith("m_", StringComparison.Ordinal))
            {
                fieldName = fieldName.Substring(2);
            }
            else if (fieldName.StartsWith("_", StringComparison.Ordinal))
            {
                fieldName = fieldName.Substring(1);
            }
            else if (fieldName.Length > 1 && fieldName[0] == 'm' && char.IsUpper(fieldName[1]))
            {
                fieldName = fieldName.Substring(1);
            }

            return UppercaseFirst(fieldName);
        }

        private static string UppercaseFirst(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return char.ToUpperInvariant(value[0]).ToString();
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string LowercaseFirst(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return char.ToLowerInvariant(value[0]).ToString();
            }

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string SanitizeFileName(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var ch in value)
            {
                builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
            }

            return builder.ToString();
        }

        private sealed class PartialTypeModel
        {
            public PartialTypeModel(
                INamedTypeSymbol typeSymbol,
                ImmutableArray<ObservablePropertyData> observableProperties,
                ImmutableArray<CommandData> commands)
            {
                this.TypeSymbol = typeSymbol;
                this.ObservableProperties = observableProperties;
                this.Commands = commands;
            }

            public INamedTypeSymbol TypeSymbol { get; }

            public ImmutableArray<ObservablePropertyData> ObservableProperties { get; }

            public ImmutableArray<CommandData> Commands { get; }
        }

        private sealed class TypeGenerationModel
        {
            public TypeGenerationModel(INamedTypeSymbol typeSymbol)
            {
                this.TypeSymbol = typeSymbol;
                this.ObservableProperties = new Dictionary<string, ObservablePropertyData>(StringComparer.Ordinal);
                this.Commands = new Dictionary<string, CommandData>(StringComparer.Ordinal);
            }

            public INamedTypeSymbol TypeSymbol { get; }

            public Dictionary<string, ObservablePropertyData> ObservableProperties { get; }

            public Dictionary<string, CommandData> Commands { get; }
        }

        private sealed class ObservablePropertyData
        {
            public ObservablePropertyData(
                string fieldName,
                string fieldTypeName,
                string propertyName,
                ImmutableArray<string> additionalNotifications)
            {
                this.FieldName = fieldName;
                this.FieldTypeName = fieldTypeName;
                this.PropertyName = propertyName;
                this.AdditionalNotifications = additionalNotifications;
            }

            public string FieldName { get; }

            public string FieldTypeName { get; }

            public string PropertyName { get; }

            public ImmutableArray<string> AdditionalNotifications { get; }
        }

        private sealed class CommandData
        {
            public CommandData(IMethodSymbol methodSymbol, string propertyName, IMethodSymbol canExecuteMethod)
            {
                this.MethodSymbol = methodSymbol;
                this.PropertyName = propertyName;
                this.CanExecuteMethod = canExecuteMethod;
            }

            public IMethodSymbol MethodSymbol { get; }

            public string PropertyName { get; }

            public IMethodSymbol CanExecuteMethod { get; }
        }
    }
}

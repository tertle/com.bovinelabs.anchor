// <copyright file="SemanticModelExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SourceGenerator.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class SemanticModelExtensions
    {
        public static IReadOnlyCollection<string> GetAccessibleNamespaces(this SemanticModel semanticModel, SyntaxNode syntaxNode)
        {
            var namespaces = new HashSet<string>();
            var root = syntaxNode.SyntaxTree.GetRoot();

            // Get all using directives in the syntax tree
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            foreach (var usingDirective in usingDirectives)
            {
                if (usingDirective.Alias != null)
                {
                    // For alias directives, get the alias name and the target's full name.
                    var aliasName = usingDirective.Alias.Name.Identifier.ValueText;
                    var targetSymbol = semanticModel.GetSymbolInfo(usingDirective.Name).Symbol;
                    if (targetSymbol != null)
                    {
                        var targetName = targetSymbol.ToDisplayString();
                        namespaces.Add($"{aliasName} = {targetName}");
                    }
                }
                else
                {
                    // Normal using directive: just get the namespace or type.
                    var symbol = semanticModel.GetSymbolInfo(usingDirective.Name).Symbol;
                    if (symbol != null)
                    {
                        namespaces.Add(symbol.ToDisplayString());
                    }
                }
            }

            // Add namespaces for all containing types and namespaces
            AddContainingNamespaces(syntaxNode, semanticModel, namespaces);

            return namespaces;
        }

        private static void AddContainingNamespaces(SyntaxNode node, SemanticModel semanticModel, HashSet<string> namespaces)
        {
            while (node != null)
            {
                if (node is NamespaceDeclarationSyntax namespaceDeclaration)
                {
                    var name = semanticModel.GetDeclaredSymbol(namespaceDeclaration)?.ToDisplayString();
                    if (name != null)
                    {
                        namespaces.Add(name);
                    }
                }
                else if (node is TypeDeclarationSyntax typeDeclaration)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                    while (symbol?.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
                    {
                        namespaces.Add(symbol.ContainingNamespace.ToDisplayString());
                        symbol = symbol.ContainingNamespace;
                    }
                }

                node = node.Parent;
            }
        }
    }
}

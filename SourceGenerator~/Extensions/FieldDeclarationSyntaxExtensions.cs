// <copyright file="FieldDeclarationSyntaxExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SourceGenerator.Extensions
{
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class FieldDeclarationSyntaxExtensions
    {
        public static bool HasAttribute(this FieldDeclarationSyntax fieldDeclaration, string attributeName)
        {
            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var name = attribute.Name.ToString().Split('.').Last();

                    if (!name.EndsWith("Attribute"))
                    {
                        name += "Attribute";
                    }

                    if (name.Equals(attributeName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetFieldName(this FieldDeclarationSyntax fieldDeclaration)
        {
            // Get the variable declaration within the field declaration
            VariableDeclarationSyntax variableDeclaration = fieldDeclaration.Declaration;

            // Get the first variable from the declaration (assuming there's at least one)
            VariableDeclaratorSyntax variable = variableDeclaration.Variables.First();

            // Return the name of the variable
            return variable.Identifier.ValueText;
        }

        public static string GetFieldType(this FieldDeclarationSyntax fieldDeclaration)
        {
            // Get the variable declaration within the field declaration
            VariableDeclarationSyntax variableDeclaration = fieldDeclaration.Declaration;

            // Get the type of the variable
            TypeSyntax typeSyntax = variableDeclaration.Type;

            // Return the type as a string
            return typeSyntax.ToString();
        }
    }
}

// <copyright file="SystemPropertyDiagnostics.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SystemPropertyGenerator
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.CodeAnalysis;

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
    internal static class SystemPropertyDiagnostics
    {
        private const string Category = "BovineLabs.Anchor.SystemProperty";

        internal static readonly DiagnosticDescriptor NotInStructDescriptor = new DiagnosticDescriptor(
            "BLSYS0001",
            "SystemProperty must be declared in a struct",
            "Field '{0}' must be declared in a struct to use [SystemProperty]",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor MissingPartialDescriptor = new DiagnosticDescriptor(
            "BLSYS0002",
            "SystemProperty source generation requires a partial struct",
            "Struct '{0}' must be declared partial to use [SystemProperty]",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidFieldDeclarationDescriptor = new DiagnosticDescriptor(
            "BLSYS0003",
            "SystemProperty field declaration is invalid",
            "Field declaration for '{0}' must declare exactly one non-static, non-const, non-readonly field to use [SystemProperty]",
            Category,
            DiagnosticSeverity.Error,
            true);

        public static Diagnostic NotInStruct(IFieldSymbol fieldSymbol, Location location)
        {
            return Diagnostic.Create(NotInStructDescriptor, location ?? fieldSymbol.Locations[0], fieldSymbol.Name);
        }

        public static Diagnostic MissingPartial(INamedTypeSymbol typeSymbol, Location location)
        {
            return Diagnostic.Create(MissingPartialDescriptor, location ?? typeSymbol.Locations[0], typeSymbol.Name);
        }

        public static Diagnostic InvalidFieldDeclaration(string fieldName, Location location)
        {
            return Diagnostic.Create(InvalidFieldDeclarationDescriptor, location, fieldName);
        }
    }
}

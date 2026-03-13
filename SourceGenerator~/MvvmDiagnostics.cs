// <copyright file="MvvmDiagnostics.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.SystemPropertyGenerator
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.CodeAnalysis;

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
    internal static class MvvmDiagnostics
    {
        private const string Category = "BovineLabs.Anchor.MVVM";

        internal static readonly DiagnosticDescriptor MissingPartialDescriptor = new DiagnosticDescriptor(
            "BLMVVM0001",
            "MVVM source generation requires a partial type",
            "Type '{0}' must be declared partial to use [ObservableProperty] or [ICommand]",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidObservableFieldDescriptor = new DiagnosticDescriptor(
            "BLMVVM0002",
            "ObservableProperty field must be writable",
            "Field '{0}' must be a non-static, non-const, non-readonly field to use [ObservableProperty]",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor GeneratedNameConflictDescriptor = new DiagnosticDescriptor(
            "BLMVVM0003",
            "Generated member name conflicts",
            "Generated member '{0}' conflicts with an existing member or another generated member in type '{1}'",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidAlsoExecuteMethodDescriptor = new DiagnosticDescriptor(
            "BLMVVM0004",
            "AlsoExecute target is invalid",
            "Method '{0}' must exist on type '{1}' and be a non-generic method with no parameters to use [AlsoExecute]",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidCommandMethodDescriptor = new DiagnosticDescriptor(
            "BLMVVM0005",
            "ICommand method signature is invalid",
            "Method '{0}' must return void and declare at most one non-ref parameter to use [ICommand]",
            Category,
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidCanExecuteMethodDescriptor = new DiagnosticDescriptor(
            "BLMVVM0006",
            "CanExecute method is invalid",
            "Method '{0}' on type '{1}' must return bool and match the [ICommand] method parameters",
            Category,
            DiagnosticSeverity.Error,
            true);

        public static Diagnostic MissingPartial(INamedTypeSymbol typeSymbol, Location location)
        {
            return Diagnostic.Create(MissingPartialDescriptor, location ?? typeSymbol.Locations[0], typeSymbol.Name);
        }

        public static Diagnostic InvalidObservableField(IFieldSymbol fieldSymbol, Location location)
        {
            return Diagnostic.Create(InvalidObservableFieldDescriptor, location ?? fieldSymbol.Locations[0], fieldSymbol.Name);
        }

        public static Diagnostic GeneratedNameConflict(INamedTypeSymbol typeSymbol, string memberName, Location location)
        {
            return Diagnostic.Create(GeneratedNameConflictDescriptor, location ?? typeSymbol.Locations[0], memberName, typeSymbol.Name);
        }

        public static Diagnostic InvalidAlsoExecuteMethod(INamedTypeSymbol typeSymbol, string methodName, Location location)
        {
            return Diagnostic.Create(InvalidAlsoExecuteMethodDescriptor, location ?? typeSymbol.Locations[0], methodName, typeSymbol.Name);
        }

        public static Diagnostic InvalidCommandMethod(IMethodSymbol methodSymbol, Location location)
        {
            return Diagnostic.Create(InvalidCommandMethodDescriptor, location ?? methodSymbol.Locations[0], methodSymbol.Name);
        }

        public static Diagnostic InvalidCanExecuteMethod(INamedTypeSymbol typeSymbol, string methodName, Location location)
        {
            return Diagnostic.Create(InvalidCanExecuteMethodDescriptor, location ?? typeSymbol.Locations[0], methodName, typeSymbol.Name);
        }
    }
}

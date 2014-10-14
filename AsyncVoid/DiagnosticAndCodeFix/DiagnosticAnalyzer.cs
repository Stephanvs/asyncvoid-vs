using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiagnosticAndCodeFix
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DiagnosticAnalyzer : ISymbolAnalyzer
    {
        public const string DiagnosticId = "AsyncVoidReturnTypeDiagnosticAndFix";
        internal const string Description = "Consider returning Task";
        internal const string MessageFormat = "Do not use void with async methods";
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = 
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public ImmutableArray<SymbolKind> SymbolKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(SymbolKind.Method);
            }
        }

        public void AnalyzeSymbol(ISymbol symbol, Compilation compilation, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            var methodSymbol = (IMethodSymbol)symbol;

            if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
            {
                var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
                addDiagnostic(diagnostic);
            }
        }
    }
}

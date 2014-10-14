using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;

namespace DiagnosticAndCodeFix
{
    [ExportCodeFixProvider(DiagnosticAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    public class CodeFixProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { DiagnosticAnalyzer.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var diagnosticSpan = diagnostics.First().Location.SourceSpan;
            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            return new[]
            {
                CodeAction.Create("Change to Task", c =>
                {
                    var identifierToken = methodDeclaration.Identifier;
                    var newReturnType = SyntaxFactory.ParseTypeName("Task");

                    var returnSyntax = SyntaxFactory.ReturnStatement(
                            SyntaxFactory.ObjectCreationExpression(newReturnType)
                            .WithNewKeyword(
                                SyntaxFactory.Token(
                                    SyntaxFactory.TriviaList(),
                                    SyntaxKind.NewKeyword,
                                    SyntaxFactory.TriviaList(
                                        SyntaxFactory.Space)))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(
                                                    "HttpStatusCode"),
                                                SyntaxFactory.IdentifierName(
                                                    "Created")))))))
                        .WithReturnKeyword(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Whitespace(
                                        "   ")),
                                SyntaxKind.ReturnKeyword,
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Space)))
                        .WithSemicolonToken(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(),
                                SyntaxKind.SemicolonToken,
                                SyntaxFactory.TriviaList()
                            ));

                    var oldReturnSyntax = methodDeclaration.DescendantNodes().OfType<ReturnStatementSyntax>().FirstOrDefault();
                    MethodDeclarationSyntax newMethod;

                    if (oldReturnSyntax != null)
                    {
                        newMethod = methodDeclaration.ReplaceNode(oldReturnSyntax, returnSyntax);
                    }
                    else
                    {
                        newMethod = methodDeclaration.AddBodyStatements(returnSyntax);
                    }

                    var replacedMethod = newMethod.ReplaceNode(newMethod.ReturnType, newReturnType);
                    var replacedRoot = root.ReplaceNode(methodDeclaration, replacedMethod);
                    var formattedRoot = Formatter.Format(replacedRoot, MSBuildWorkspace.Create());

                    return Task.FromResult(document.WithSyntaxRoot(formattedRoot));
                })
            };
        }
    }
}
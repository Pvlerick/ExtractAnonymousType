using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using ExtractAnonymousType;
using Xunit;

namespace ExtractAnonymousType.Test
{
    public class ExtractAnonymousTypeAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public void NoDiagnosticWhenEmptyCode()
        {
            // Fixture setup
            var test = @"";
            // Exercise system & Verify outcome
            VerifyCSharpDiagnostic(test);
            // Teardown 
        }

        [Fact]
        public void CorrectDiagnosticWhenAnonymousTypeIsUsed()
        {
            // Fixture setup
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class C
        {
            void M()
            {
                var a = new { P = ""dummy"" };
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "ExtractAnonymousType",
                Message = "Anonymous type used",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 25)
                        }
            };
            // Exercise system & Verify outcome
            VerifyCSharpDiagnostic(test, expected);
            // Teardown 
        }


        [Fact]
        public void CorrectDiagnosticWhenAnonymousTypeInProjection()
        {
            // Fixture setup
            var test = @"
using System;
using System.Linq;

namespace N
{
    class C
    {
        void M()
        {
            var q = Enumerable.Range(0, 5)
                .Select(i => new { Qux = i });
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ExtractAnonymousType",
                Message = "Anonymous type used",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 30)
                        }
            };
            // Exercise system & Verify outcome
            VerifyCSharpDiagnostic(test, expected);
            // Teardown 
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractAnonymousTypeAnalyzer();
        }
    }
}
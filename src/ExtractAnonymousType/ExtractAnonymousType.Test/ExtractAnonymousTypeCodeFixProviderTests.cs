using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using ExtractAnonymousType;
using Xunit;

namespace ExtractAnonymousType.Test
{
    public class ExtractAnonymousTypeCodeFixProviderTests : CodeFixVerifier
    {
        [Fact]
        public void CorrectSyntaxProducedWhenApplyingFixOnAnonymousType()
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

            var fixtest = @"
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
    class Anon1 { public string P { get; set; } }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown 
        }

        [Fact(Skip = "Not implemented yet")]
        public void CorrectSyntaxProducedWhenApplyingFixOnAnonymousWithTwoProperties()
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            // Teardown 
        }

        [Fact(Skip = "Not implemented yet")]
        public void CorrectSyntaxProducedWhenApplyingFixOnAnonymousTypeUsedInTwoDifferentPlaces()
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            // Teardown 
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractAnonymousTypeAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExtractAnonymousTypeCodeFixProvider();
        }
    }
}

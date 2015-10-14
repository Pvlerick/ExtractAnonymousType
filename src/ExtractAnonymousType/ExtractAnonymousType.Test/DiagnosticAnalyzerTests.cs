﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using ExtractAnonymousType;
using Xunit;

namespace ExtractAnonymousType.Test
{
    public class UnitTest : CodeFixVerifier
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
                Message = String.Format("Variable '{0}' is an anonymous type", "a"),
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 21)
                        }
            };
            // Exercise system & Verify outcome
            VerifyCSharpDiagnostic(test, expected);
            // Teardown 
        }

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
            
        class A
        {
            public string P { get; set; }
        }
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

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExtractAnonymousTypeCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractAnonymousTypeAnalyzer();
        }
    }
}
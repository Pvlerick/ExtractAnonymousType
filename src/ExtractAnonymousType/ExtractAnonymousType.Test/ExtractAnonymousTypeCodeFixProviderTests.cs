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
        public void CorrectSyntaxProducedWithSingleProperty()
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
            var a = new { Foo = ""Bar"" };
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
            var a = new Anon1 { Foo = ""Bar"" };
        }
    }
    class Anon1
    {
        public string Foo { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }

        [Fact]
        public void CorrectSyntaxProducedWWithTwoProperties()
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
            var a = new { Foo = ""Bar"", Qux = 42 };
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
            var a = new Anon1 { Foo = ""Bar"", Qux = 42 };
        }
    }
    class Anon1
    {
        public string Foo { get; set; }
        public int Qux { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }

        [Fact]
        public void CorrectSyntaxWithNonPrimitiveTypePropertyWithoutUsing()
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
            var sb = new System.Text.StringBuilder();
            var a = new { Foo = sb };
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
            var sb = new System.Text.StringBuilder();
            var a = new Anon1 { Foo = sb };
        }
    }
    class Anon1
    {
        public System.Text.StringBuilder Foo { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }

        [Fact(Skip = "Not implemented yet")]
        public void CorrectSyntaxWithNonPrimitiveTypePropertyWithUsing()
        {
            // Fixture setup
            var test = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class C
    {
        void M()
        {
            var sb = new System.Text.StringBuilder();
            var a = new { Foo = sb };
        }
    }
}";

            var fixtest = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class C
    {
        void M()
        {
            var sb = new System.Text.StringBuilder();
            var a = new Anon1 { Foo = sb };
        }
    }
    class Anon1
    {
        public StringBuilder Foo { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }

        [Fact]
        public void CorrectSyntaxProducedWhenApplyingFixOnAnonymousTypeUsedInTwoDifferentPlaces()
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
            var a = new { Foo = ""Bar"", Qux = 42 };
            var b = new { Foo = ""B4r"", Qux = 43 };
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
            var a = new Anon1 { Foo = ""Bar"", Qux = 42 };
            var b = new Anon1 { Foo = ""B4r"", Qux = 43 };
        }
    }
    class Anon1
    {
        public string Foo { get; set; }
        public int Qux { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }


        [Fact(Skip = "Not implemented yet")]
        public void CorrectSyntaxWhenDefaultClassNameAlreadyExists()
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

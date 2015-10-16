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

namespace N
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

namespace N
{
    class C
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"" };
        }
    }

    class MyType
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

namespace N
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

namespace N
{
    class C
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"", Qux = 42 };
        }
    }

    class MyType
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

namespace N
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

namespace N
{
    class C
    {
        void M()
        {
            var sb = new System.Text.StringBuilder();
            var a = new MyType { Foo = sb };
        }
    }

    class MyType
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

namespace N
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

namespace N
{
    class C
    {
        void M()
        {
            var sb = new System.Text.StringBuilder();
            var a = new MyType { Foo = sb };
        }
    }

    class MyType
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

namespace N
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

namespace N
{
    class C
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"", Qux = 42 };
            var b = new MyType { Foo = ""B4r"", Qux = 43 };
        }
    }

    class MyType
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
        public void CorrectSyntaxWhenDefaultClassNameAlreadyExists()
        {
            // Fixture setup
            var test = @"
using System;

namespace N
{
    class C
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"" };
            var b = new { Qux = 43 };
        }
    }

    class MyType
    {
        public string Foo { get; set; }
    }
}";

            var fixtest = @"
using System;

namespace N
{
    class C
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"" };
            var b = new MyType1 { Qux = 43 };
        }
    }

    class MyType1
    {
        public int Qux { get; set; }
    }

    class MyType
    {
        public string Foo { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }

        [Fact]
        public void CorrectSyntaxWhenInsideStructInsteadOfClass()
        {
            // Fixture setup
            var test = @"
using System;

namespace N
{
    struct S
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"" };
        }
    }

    class MyType
    {
        public string Foo { get; set; }
    }
}";

            var fixtest = @"
using System;

namespace N
{
    struct S
    {
        void M()
        {
            var a = new MyType { Foo = ""Bar"" };
        }
    }

    class MyType
    {
        public string Foo { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
            // Teardown
        }

        [Fact(Skip = "Not implemented yet")]
        public void CorrectSyntaxWhenStatementIsInsideProjection()
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

            var fixtest = @"
using System;
using System.Linq;

namespace N
{
    class C
    {
        void M()
        {
            var q = Enumerable.Range(0, 5)
                .Select(i => new MyType { Qux = i });
        }
    }

    class MyType
    {
        public int Qux { get; set; }
    }
}";

            // Exercise system & Verify outcome
            VerifyCSharpFix(test, fixtest);
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

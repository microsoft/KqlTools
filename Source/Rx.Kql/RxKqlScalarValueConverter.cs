// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomTypes;
    using Kusto.Language.Syntax;

    class ListRxKqlScalarValueConverter : KqlSyntaxVisitor<List<RxKqlScalarValue>>
    {
        public override List<RxKqlScalarValue> VisitExtendOperator(Kusto.Language.Syntax.ExtendOperator node)
        {
            var visitor = new RxKqlScalarValueConverter();
            return node.Expressions.Select(e => e.Element.Visit(visitor)).ToList();
        }

        public override List<RxKqlScalarValue> VisitExpressionStatement(ExpressionStatement node)
        {
            return node.Expression.Visit(this);
        }
    }

    class RxKqlScalarValueConverter : KqlSyntaxVisitor<RxKqlScalarValue>
    {
        private int NamelessCounter = 0;

        public override RxKqlScalarValue VisitSimpleNamedExpression(SimpleNamedExpression node)
        {
            return new RxKqlScalarValue
            {
                Left = node.Name.SimpleName,
                Right = node.Expression.Visit(new ScalarValueConverter())
            };
        }

        public override RxKqlScalarValue VisitIdentifierNameReference(IdentifierNameReference node)
        {
            return new RxKqlScalarValue
            {
                Left = node.SimpleName,
                Right = node.Visit(new ScalarValueConverter())
            };
        }

        public override RxKqlScalarValue VisitLiteralExpression(LiteralExpression node)
        {
            return new RxKqlScalarValue
            {
                Left = $"Column{++NamelessCounter}",
                Right = node.Visit(new ScalarValueConverter())
            };
        }
    }
}

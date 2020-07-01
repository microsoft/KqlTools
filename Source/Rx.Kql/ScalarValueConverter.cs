// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Linq;
    using System.Reactive.Kql.CustomTypes;
    using Expressions;
    using Functions;
    using Kusto.Language.Syntax;

    class ScalarValueConverter : KqlSyntaxVisitor<ScalarValue>
    {
        public override ScalarValue VisitSummarizeOperator(Kusto.Language.Syntax.SummarizeOperator node)
        {
            ScalarSummarizer scalarSummarizer = new ScalarSummarizer();

            var aggregates = node.Aggregates;
            foreach (var aggregate in aggregates)
            {
                RxKqlScalarValue scalarValue = aggregate.Visit(this) as RxKqlScalarValue;
                scalarSummarizer.Aggregations.Add(scalarValue.Left, scalarValue.Right as AggregationFunction);
            }

            scalarSummarizer.GroupingElements = node.ByClause.Expressions.Visit(this) as ScalarValueList;

            return scalarSummarizer;
        }

        public override ScalarValue VisitList(SyntaxList list)
        {
            ScalarValueList valueList = new ScalarValueList();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is SeparatedElement separatedElement)
                {
                    valueList.List.Add(separatedElement.Visit(this));
                }

                if (list[i] is IdentifierNameReference identifierNameReference)
                {
                    valueList.List.Add(identifierNameReference.Visit(this));
                }
            }

            if (valueList.List.Count > 0)
            {
                return valueList;
            }

            return base.VisitList(list);
        }

        public override ScalarValue VisitSummarizeByClause(SummarizeByClause node)
        {
            return node.Expressions.Visit(this);
        }

        public override ScalarValue VisitSeparatedElement(SeparatedElement node)
        {
            Kusto.Language.Syntax.Query query = node.Root as Kusto.Language.Syntax.Query;
            var statement = query.Statements[0];
            var expressionStatement = statement.Element as ExpressionStatement;

            if (expressionStatement.Expression is Kusto.Language.Syntax.SummarizeOperator)
            {
                if (node.Parent.Parent is SummarizeByClause &&
                    node.Element is FunctionCallExpression functionCallExpression)
                {
                    return functionCallExpression.Visit(this);
                }

                if (node.Parent.Parent is SummarizeByClause &&
                    node.Element is IdentifierNameReference identifierNameReference)
                {
                    return identifierNameReference.Visit(this);
                }

                if (node.Element is FunctionCallExpression functionCallExpression1)
                {
                    AggregationFunction aggregationFunction =
                        VisitFunctionCallExpression(functionCallExpression1) as AggregationFunction;

                    RxKqlScalarValue retVal = new RxKqlScalarValue
                    {
                        Left = aggregationFunction.Name,
                        Right = aggregationFunction
                    };

                    return retVal;
                }

                if (node.Element is SimpleNamedExpression simpleNamedExpression)
                {
                    AggregationFunction aggregationFunction =
                        simpleNamedExpression.Expression.Visit(this) as AggregationFunction;

                    RxKqlScalarValue retVal = new RxKqlScalarValue
                    {
                        Left = simpleNamedExpression.Name.SimpleName,
                        Right = aggregationFunction
                    };

                    return retVal;
                }
            }

            return base.VisitSeparatedElement(node);
        }

        public override ScalarValue VisitTypeOfLiteralExpression(TypeOfLiteralExpression node)
        {
            return new ScalarType
            {
                Value = ((PrimitiveTypeExpression)node.Types[0].Element).Type.Text
            };
        }

        public override ScalarValue VisitLiteralExpression(LiteralExpression node)
        {
            return new ScalarConst
            {
                Value = node.LiteralValue
            };
        }

        public override ScalarValue VisitCompoundStringLiteralExpression(CompoundStringLiteralExpression node)
        {
            return new ScalarConst
            {
                Value = node.LiteralValue // concatenation of the string literals
            };
        }

        public override ScalarValue VisitIdentifierNameReference(IdentifierNameReference node)
        {
            return new ScalarProperty(node.Identifier.Text);
        }

        public override ScalarValue VisitBinaryExpression(Kusto.Language.Syntax.BinaryExpression node)
        {
            var left = node.Left.Visit(this);
            var right = node.Right.Visit(this);
            var op = node.Operator.Text;
            return BinaryExpressionFactory.Create(op, left, right);
        }

        public override ScalarValue VisitFunctionCallExpression(FunctionCallExpression node)
        {
            var functionName = node.Name.SimpleName;
            var args = node.ArgumentList.Expressions.Select(e => e.Element.Visit(this)).ToList();

            Kusto.Language.Syntax.Query query = node.Root as Kusto.Language.Syntax.Query;
            var statement = query.Statements[0];
            var expressionStatement = statement.Element as ExpressionStatement;

            if (expressionStatement.Expression is Kusto.Language.Syntax.SummarizeOperator)
            {
                if (node.Parent.Parent.Parent is SummarizeByClause)
                {
                    return ScalarFunctionFactory.Create(functionName, args);
                }

                return AggregationFunctionFactory.Create(functionName, args);
            }
            else
            {
                return ScalarFunctionFactory.Create(functionName, args);
            }
        }

        public override ScalarValue VisitParenthesizedExpression(ParenthesizedExpression node)
        {
            return node.Expression.Visit(this);
        }

        public override ScalarValue VisitInExpression(Kusto.Language.Syntax.InExpression node)
        {
            var left = node.Left.Visit(this);
            var right = node.Right.Expressions.Select(e => e.Element.Visit(this)).ToList();
            if (node.Operator.Text.Contains("!"))
            {
                return new Expressions.NotInExpression
                {
                    Left = left,
                    Right = right,
                    CaseInsensitive = node.Operator.Text.Contains("~")
                };
            }
            return new Expressions.InExpression
            {
                Left = left,
                Right = right,
                CaseInsensitive = node.Operator.Text.Contains("~")
            };
        }

        public override ScalarValue VisitBetweenExpression(Kusto.Language.Syntax.BetweenExpression node)
        {
            var left = node.Left.Visit(this);
            var low = node.Right.First.Visit(this);
            var high = node.Right.Second.Visit(this);
            if (node.Operator.Text.Contains("!"))
            {
                return new NotBetweenExpression
                {
                    Value = left,
                    Low = low,
                    High = high
                };
            }
            return new Expressions.BetweenExpression
            {
                Value = left,
                Low = low,
                High = high
            };
        }

        public override ScalarValue VisitPathExpression(PathExpression node)
        {
            return new ScalarPath
            {
                Element = node.Expression.Visit(this),
                Selector = node.Selector.Visit(this)
            };
        }

        public override ScalarValue VisitElementExpression(ElementExpression node)
        {
            return new ScalarIndexedElement
            {
                Element = node.Expression.Visit(this),
                Selector = node.Selector.Visit(this)
            };
        }

        public override ScalarValue VisitBrackettedExpression(BrackettedExpression node)
        {
            return node.Expression.Visit(this);
        }

        public override ScalarValue VisitExpressionStatement(ExpressionStatement node)
        {
            return node.Expression.Visit(this);
        }
    }
}
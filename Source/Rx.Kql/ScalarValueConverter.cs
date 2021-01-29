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
                RxKqlScalarValue scalarValue = aggregate.Accept(this) as RxKqlScalarValue;
                scalarSummarizer.Aggregations.Add(scalarValue.Left, scalarValue.Right as AggregationFunction);
            }

            scalarSummarizer.GroupingElements = node.ByClause.Expressions.Accept(this) as ScalarValueList;

            return scalarSummarizer;
        }

        public override ScalarValue VisitScanOperator(ScanOperator node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitScanDeclareClause(ScanDeclareClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitScanOrderByClause(ScanOrderByClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitScanPartitionByClause(ScanPartitionByClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitScanStep(ScanStep node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitScanComputationClause(ScanComputationClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitScanAssignment(ScanAssignment node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitUnknownCommand(UnknownCommand node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitCustomCommand(CustomCommand node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitCommandBlock(CommandBlock node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitList(SyntaxList list)
        {
            ScalarValueList valueList = new ScalarValueList();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is SeparatedElement separatedElement)
                {
                    valueList.List.Add(separatedElement.Accept(this));
                }

                if (list[i] is NameReference identifierNameReference)
                {
                    valueList.List.Add(identifierNameReference.Accept(this));
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
            return node.Expressions.Accept(this);
        }

        public override ScalarValue VisitSeparatedElement(SeparatedElement node)
        {
            Kusto.Language.Syntax.QueryBlock query = node.Root as Kusto.Language.Syntax.QueryBlock;
            var statement = query.Statements[0];
            var expressionStatement = statement.Element as ExpressionStatement;

            if (expressionStatement.Expression is Kusto.Language.Syntax.SummarizeOperator)
            {
                if (node.Parent.Parent is SummarizeByClause &&
                    node.Element is FunctionCallExpression functionCallExpression)
                {
                    return functionCallExpression.Accept(this);
                }

                if (node.Parent.Parent is SummarizeByClause &&
                    node.Element is NameReference identifierNameReference)
                {
                    return identifierNameReference.Accept(this);
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
                        simpleNamedExpression.Expression.Accept(this) as AggregationFunction;

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

        public override ScalarValue VisitQueryBlock(QueryBlock node)
        {
            throw new NotImplementedException();
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

        public override ScalarValue VisitTokenName(TokenName node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitBracedName(BracedName node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitBracketedWildcardedName(BracketedWildcardedName node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitNameReference(NameReference node)
        {
            return new ScalarProperty(node.Name.SimpleName);
        }

        public override ScalarValue VisitBinaryExpression(Kusto.Language.Syntax.BinaryExpression node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            var op = node.Operator.Text;
            return BinaryExpressionFactory.Create(op, left, right);
        }

        public override ScalarValue VisitFunctionCallExpression(FunctionCallExpression node)
        {
            var functionName = node.Name.SimpleName;
            var args = node.ArgumentList.Expressions.Select(e => e.Element.Accept(this)).ToList();

            Kusto.Language.Syntax.QueryBlock query = node.Root as Kusto.Language.Syntax.QueryBlock;
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

        public override ScalarValue VisitMaterializedViewCombineExpression(MaterializedViewCombineExpression node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitMaterializedViewCombineNameClause(MaterializedViewCombineNameClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitMaterializedViewCombineClause(MaterializedViewCombineClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitParenthesizedExpression(ParenthesizedExpression node)
        {
            return node.Expression.Accept(this);
        }

        public override ScalarValue VisitInExpression(Kusto.Language.Syntax.InExpression node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Expressions.Select(e => e.Element.Accept(this)).ToList();
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

        public override ScalarValue VisitHasAnyExpression(HasAnyExpression node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitBetweenExpression(Kusto.Language.Syntax.BetweenExpression node)
        {
            var left = node.Left.Accept(this);
            var low = node.Right.First.Accept(this);
            var high = node.Right.Second.Accept(this);
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
                Element = node.Expression.Accept(this),
                Selector = node.Selector.Accept(this)
            };
        }

        public override ScalarValue VisitElementExpression(ElementExpression node)
        {
            return new ScalarIndexedElement
            {
                Element = node.Expression.Accept(this),
                Selector = node.Selector.Accept(this)
            };
        }

        public override ScalarValue VisitBracketedExpression(BracketedExpression node)
        {
            return node.Expression.Accept(this);
        }

        public override ScalarValue VisitExpressionStatement(ExpressionStatement node)
        {
            return node.Expression.Accept(this);
        }

        public override ScalarValue VisitMakeSeriesFromClause(MakeSeriesFromClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitMakeSeriesToClause(MakeSeriesToClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitMakeSeriesStepClause(MakeSeriesStepClause node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitParseWhereOperator(ParseWhereOperator node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitPartitionQuery(PartitionQuery node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitPartitionScope(PartitionScope node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitPartitionSubquery(PartitionSubquery node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitProjectAwayOperator(Kusto.Language.Syntax.ProjectAwayOperator node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitProjectKeepOperator(Kusto.Language.Syntax.ProjectKeepOperator node)
        {
            throw new NotImplementedException();
        }

        public override ScalarValue VisitProjectReorderOperator(ProjectReorderOperator node)
        {
            throw new NotImplementedException();
        }
    }
}
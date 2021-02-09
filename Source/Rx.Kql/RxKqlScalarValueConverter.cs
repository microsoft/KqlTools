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
            return node.Expressions.Select(e => e.Element.Accept(visitor)).ToList();
        }

        public override List<RxKqlScalarValue> VisitMakeSeriesFromClause(MakeSeriesFromClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitMakeSeriesToClause(MakeSeriesToClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitMakeSeriesStepClause(MakeSeriesStepClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitParseWhereOperator(ParseWhereOperator node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitPartitionQuery(PartitionQuery node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitPartitionScope(PartitionScope node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitPartitionSubquery(PartitionSubquery node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitProjectOperator(Kusto.Language.Syntax.ProjectOperator node)
        {
            var visitor = new RxKqlScalarValueConverter();
            return node.Expressions.Select(e => e.Element.Accept(visitor)).ToList();
        }

        public override List<RxKqlScalarValue> VisitProjectAwayOperator(Kusto.Language.Syntax.ProjectAwayOperator node)
        {
            var visitor = new RxKqlScalarValueConverter();
            return node.Expressions.Select(e => e.Element.Accept(visitor)).ToList();
        }

        public override List<RxKqlScalarValue> VisitProjectKeepOperator(Kusto.Language.Syntax.ProjectKeepOperator node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitProjectReorderOperator(ProjectReorderOperator node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitExpressionStatement(ExpressionStatement node)
        {
            return node.Expression.Accept(this);
        }

        public override List<RxKqlScalarValue> VisitScanOperator(ScanOperator node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitScanDeclareClause(ScanDeclareClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitScanOrderByClause(ScanOrderByClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitScanPartitionByClause(ScanPartitionByClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitScanStep(ScanStep node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitScanComputationClause(ScanComputationClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitScanAssignment(ScanAssignment node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitUnknownCommand(UnknownCommand node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitCustomCommand(CustomCommand node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitCommandBlock(CommandBlock node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitQueryBlock(QueryBlock node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitTokenName(TokenName node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitBracedName(BracedName node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitBracketedWildcardedName(BracketedWildcardedName node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitHasAnyExpression(HasAnyExpression node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitMaterializedViewCombineExpression(MaterializedViewCombineExpression node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitMaterializedViewCombineNameClause(MaterializedViewCombineNameClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitMaterializedViewCombineClause(MaterializedViewCombineClause node)
        {
            throw new NotImplementedException();
        }

        public override List<RxKqlScalarValue> VisitPathExpression(PathExpression node)
        {
            throw new NotImplementedException();
        }
    }

    class RxKqlScalarValueConverter : KqlSyntaxVisitor<RxKqlScalarValue>
    {
        private int NamelessCounter = 0;

        public override RxKqlScalarValue VisitHasAnyExpression(HasAnyExpression node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitMaterializedViewCombineExpression(MaterializedViewCombineExpression node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitMaterializedViewCombineNameClause(MaterializedViewCombineNameClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitMaterializedViewCombineClause(MaterializedViewCombineClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitSimpleNamedExpression(SimpleNamedExpression node)
        {
            return new RxKqlScalarValue
            {
                Left = node.Name.SimpleName,
                Right = node.Expression.Accept(new ScalarValueConverter())
            };
        }

        public override RxKqlScalarValue VisitNameReference(NameReference node)
        {
            return new RxKqlScalarValue
            {
                Left = node.SimpleName,
                Right = node.Accept(new ScalarValueConverter())
            };
        }

        public override RxKqlScalarValue VisitQueryBlock(QueryBlock node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitTokenName(TokenName node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitBracedName(BracedName node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitBracketedWildcardedName(BracketedWildcardedName node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitLiteralExpression(LiteralExpression node)
        {
            return new RxKqlScalarValue
            {
                Left = $"Column{++NamelessCounter}",
                Right = node.Accept(new ScalarValueConverter())
            };
        }

        public override RxKqlScalarValue VisitPathExpression(PathExpression node)
        {
            return new RxKqlScalarValue
            {
                Left = node.ToString().Trim().Replace(" . ", "_"),
                Right = node.Accept(new ScalarValueConverter())
            };
        }

        public override RxKqlScalarValue VisitMakeSeriesFromClause(MakeSeriesFromClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitMakeSeriesToClause(MakeSeriesToClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitMakeSeriesStepClause(MakeSeriesStepClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitParseWhereOperator(ParseWhereOperator node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitPartitionQuery(PartitionQuery node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitPartitionScope(PartitionScope node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitPartitionSubquery(PartitionSubquery node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitProjectAwayOperator(Kusto.Language.Syntax.ProjectAwayOperator node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitProjectKeepOperator(Kusto.Language.Syntax.ProjectKeepOperator node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitProjectReorderOperator(ProjectReorderOperator node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanOperator(ScanOperator node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanDeclareClause(ScanDeclareClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanOrderByClause(ScanOrderByClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanPartitionByClause(ScanPartitionByClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanStep(ScanStep node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanComputationClause(ScanComputationClause node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitScanAssignment(ScanAssignment node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitUnknownCommand(UnknownCommand node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitCustomCommand(CustomCommand node)
        {
            throw new NotImplementedException();
        }

        public override RxKqlScalarValue VisitCommandBlock(CommandBlock node)
        {
            throw new NotImplementedException();
        }
    }
}
